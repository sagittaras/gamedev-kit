#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Sagittaras.DevelopmentKit.Editor
{
    /// <summary>
    ///     One run of the opt-in pre-image build of srcs/csharp (see Directory.Build.targets), for the whole solution
    ///     or a single project. While it runs, it shows in Unity's Background Tasks how many projects are built and
    ///     which one finished last, and it can be cancelled from there. The build does not touch the AssetDatabase —
    ///     reimporting the refreshed Plugins/ folders is up to the caller.
    /// </summary>
    internal sealed class PreImageBuild
    {
        /// <summary>
        ///     The line MSBuild writes when a project is built, e.g. "Sagittaras.Dices -> C:\...\Sagittaras.Dices.dll".
        /// </summary>
        private static readonly Regex BuiltProject = new(@"^(\S+) -> ");

        private static readonly Regex SolutionProject = new(@"^Project\(""\{[^}]+\}""\) = ""[^""]+"", ""[^""]+\.csproj""", RegexOptions.Multiline);
        private static readonly Regex ProjectReference = new(@"<ProjectReference\s+Include=""([^""]+)""");

        private readonly string _target;
        private readonly List<string> _output = new();
        private int _builtProjects;
        private bool _started;

        private PreImageBuild(string target, string label)
        {
            _target = target;
            Label = label;
        }

        /// <summary>
        ///     The srcs/csharp folder, a sibling of this Unity project.
        /// </summary>
        private static string CSharpDirectory => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "csharp"));

        /// <summary>
        ///     What is being built, as shown to the user.
        /// </summary>
        private string Label { get; }

        /// <summary>
        ///     Build of the whole solution — refreshes the pre-image of every package.
        /// </summary>
        public static PreImageBuild ForSolution()
        {
            return new PreImageBuild("GameDevKit.sln", "all packages");
        }

        /// <summary>
        ///     Build of a single project and its kit dependencies, e.g. <c>Sagittaras.Dices</c>.
        /// </summary>
        public static PreImageBuild ForProject(string project)
        {
            return new PreImageBuild(Path.Combine(project, $"{project}.csproj"), project);
        }

        /// <summary>
        ///     Runs the build. Call it on the main thread — progress reports are posted back to it.
        /// </summary>
        /// <exception cref="InvalidOperationException">The build has already been run.</exception>
        public async Task<PreImageBuildResult> RunAsync()
        {
            if (_started)
            {
                throw new InvalidOperationException($"Pre-image build of {Label} has already been run.");
            }

            _started = true;
            SynchronizationContext? mainThread = SynchronizationContext.Current;
            int totalProjects = CountProjects();
            int progress = Progress.Start(
                $"Pre-image build of {Label}",
                "Restoring",
                totalProjects > 0 ? Progress.Options.None : Progress.Options.Indefinite);
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                using Process process = CreateProcess();
                process.OutputDataReceived += (_, e) => OnOutput(e.Data, progress, totalProjects, mainThread);
                process.ErrorDataReceived += (_, e) => OnOutput(e.Data, progress, totalProjects, mainThread);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool cancelled = false;
                Progress.RegisterCancelCallback(progress, () =>
                {
                    cancelled = true;
                    Kill(process);
                    return true;
                });

                await Task.Run(process.WaitForExit);

                PreImageBuildStatus status = cancelled
                    ? PreImageBuildStatus.Cancelled
                    : process.ExitCode == 0 ? PreImageBuildStatus.Succeeded : PreImageBuildStatus.Failed;

                return Finish(progress, status, process.ExitCode, stopwatch);
            }
            catch (Win32Exception)
            {
                lock (_output)
                {
                    _output.Add("'dotnet' was not found on the PATH Unity was started with — the pre-image build needs the .NET SDK.");
                }

                return Finish(progress, PreImageBuildStatus.Failed, null, stopwatch);
            }
        }

        private Process CreateProcess()
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo(
                    "dotnet",
                    // Classic logger for parseable output; no node reuse, so no MSBuild nodes outlive the build.
                    $"build \"{_target}\" -c Release -p:UpdateUnityPackage=true -nodeReuse:false -tl:off")
                {
                    WorkingDirectory = CSharpDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };
        }

        /// <summary>
        ///     Collects a line of the build output; called on the process' reader threads.
        /// </summary>
        private void OnOutput(string? data, int progress, int totalProjects, SynchronizationContext? mainThread)
        {
            if (data == null)
            {
                return;
            }

            string line = data.Trim();
            Match built = BuiltProject.Match(line);
            int builtProjects;

            lock (_output)
            {
                _output.Add(line);
                builtProjects = built.Success ? ++_builtProjects : 0;
            }

            if (builtProjects == 0)
            {
                return;
            }

            string description = $"Built {built.Groups[1].Value}";
            Post(mainThread, () => ReportProgress(progress, builtProjects, totalProjects, description));
        }

        private static void ReportProgress(int progress, int builtProjects, int totalProjects, string description)
        {
            if (!IsRunning(progress))
            {
                return;
            }

            if (totalProjects > 0)
            {
                Progress.Report(progress, Math.Min(builtProjects, totalProjects), totalProjects, description);
            }
            else
            {
                Progress.SetDescription(progress, description);
            }
        }

        private PreImageBuildResult Finish(int progress, PreImageBuildStatus status, int? exitCode, Stopwatch stopwatch)
        {
            // A cancelled task is already finished by Unity once the cancel callback accepts it.
            if (IsRunning(progress))
            {
                Progress.Finish(progress, status switch
                {
                    PreImageBuildStatus.Succeeded => Progress.Status.Succeeded,
                    PreImageBuildStatus.Cancelled => Progress.Status.Canceled,
                    _ => Progress.Status.Failed,
                });
            }

            lock (_output)
            {
                return new PreImageBuildResult(Label, status, exitCode, _output.ToArray(), stopwatch.Elapsed);
            }
        }

        private static bool IsRunning(int progress)
        {
            return Progress.Exists(progress) && Progress.GetStatus(progress) == Progress.Status.Running;
        }

        private static void Post(SynchronizationContext? mainThread, Action action)
        {
            if (mainThread == null)
            {
                action();
                return;
            }

            mainThread.Post(_ => action(), null);
        }

        private static void Kill(Process process)
        {
            try
            {
                process.Kill();
            }
            catch (InvalidOperationException)
            {
                // The build finished before it could be killed.
            }
        }

        /// <summary>
        ///     Number of projects the build goes through, so the progress can count them; 0 when unknown.
        /// </summary>
        private int CountProjects()
        {
            string target = Path.Combine(CSharpDirectory, _target);
            if (!File.Exists(target))
            {
                return 0;
            }

            if (Path.GetExtension(target) == ".sln")
            {
                return SolutionProject.Matches(File.ReadAllText(target)).Count;
            }

            HashSet<string> projects = new();
            CollectProjects(target, projects);
            return projects.Count;
        }

        /// <summary>
        ///     Adds the project and, recursively, every project it references.
        /// </summary>
        private static void CollectProjects(string project, ISet<string> projects)
        {
            string path = Path.GetFullPath(project);
            if (!File.Exists(path) || !projects.Add(path))
            {
                return;
            }

            string directory = Path.GetDirectoryName(path)!;
            foreach (Match reference in ProjectReference.Matches(File.ReadAllText(path)).Cast<Match>())
            {
                CollectProjects(Path.Combine(directory, reference.Groups[1].Value.Replace('\\', Path.DirectorySeparatorChar)), projects);
            }
        }
    }
}

#nullable enable
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Sagittaras.DevelopmentKit.Editor
{
    /// <summary>
    ///     Refreshes the Unity package pre-images without leaving Unity. Runs the opt-in pre-image build of
    ///     srcs/csharp (see Directory.Build.targets) for the whole kit or for a single package, and reimports
    ///     the refreshed Plugins/ folders once it finishes.
    /// </summary>
    /// <remarks>
    ///     Unity has no public API for menu items created at runtime, so every package has its own item below.
    ///     A package skeleton without one is reported on every domain reload.
    /// </remarks>
    internal static class PreImageBuilder
    {
        private const string PackagePrefix = "com.sagittaras.gamedevkit.";
        private const string BuildMenu = "Tools/Sagittaras/Build Pre-image/";
        private const string BuildAllMenu = BuildMenu + "All Packages";
        private const string PackageContextMenu = "Assets/Sagittaras/Build Package Pre-image";

        /// <summary>
        ///     Priority of "All Packages", listed first in the Build Pre-image menu.
        /// </summary>
        private const int AllPriority = 0;

        /// <summary>
        ///     Priority of the package items — more than 10 above "All Packages", so Unity separates them.
        /// </summary>
        private const int PackagePriority = 20;

        /// <summary>
        ///     Whether a build is running — only one at a time, both builds write into the same Plugins/ folders.
        /// </summary>
        private static bool _isBuilding;

        /// <summary>
        ///     The srcs/csharp folder, a sibling of this Unity project.
        /// </summary>
        private static string CSharpDirectory => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "csharp"));

        [MenuItem(BuildAllMenu, priority = AllPriority)]
        private static void BuildAll()
        {
            Build("GameDevKit.sln", "all packages");
        }

        [MenuItem(BuildMenu + "Sagittaras.Dices", priority = PackagePriority)]
        private static void BuildDices()
        {
            BuildProject("Sagittaras.Dices");
        }

        [MenuItem(BuildMenu + "Sagittaras.GuardClauses", priority = PackagePriority)]
        private static void BuildGuardClauses()
        {
            BuildProject("Sagittaras.GuardClauses");
        }

        [MenuItem(BuildAllMenu, true)]
        [MenuItem(BuildMenu + "Sagittaras.Dices", true)]
        [MenuItem(BuildMenu + "Sagittaras.GuardClauses", true)]
        private static bool CanBuild()
        {
            return !_isBuilding;
        }

        [MenuItem(PackageContextMenu)]
        private static void BuildSelectedPackage()
        {
            string package = SelectedPackage() ?? throw new InvalidOperationException("No kit package is selected.");
            BuildProject(ProjectName(package));
        }

        [MenuItem(PackageContextMenu, true)]
        private static bool CanBuildSelectedPackage()
        {
            return !_isBuilding && SelectedPackage() != null;
        }

        /// <summary>
        ///     Reports package skeletons that have no item in the Build Pre-image menu yet.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void ReportPackagesWithoutMenuItem()
        {
            string[] listed = TypeCache.GetMethodsWithAttribute<MenuItem>()
                .Where(method => method.DeclaringType == typeof(PreImageBuilder))
                .SelectMany(method => method.GetCustomAttributes(typeof(MenuItem), false).Cast<MenuItem>())
                .Select(item => item.menuItem)
                .Where(path => path.StartsWith(BuildMenu, StringComparison.Ordinal))
                .Select(path => path.Substring(BuildMenu.Length))
                .ToArray();

            string packages = Path.Combine(Application.dataPath, "..", "Packages");
            foreach (string skeleton in Directory.GetDirectories(packages, PackagePrefix + "*"))
            {
                string project = ProjectName(Path.GetFileName(skeleton));
                if (File.Exists(Path.Combine(skeleton, "package.json")) && !listed.Contains(project))
                {
                    Debug.LogWarning($"Package {Path.GetFileName(skeleton)} has no item in {BuildMenu.TrimEnd('/')} — add {project} to {nameof(PreImageBuilder)}.");
                }
            }
        }

        /// <summary>
        ///     Name of the kit package the selected asset in the Project window belongs to.
        /// </summary>
        private static string? SelectedPackage()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            string? name = string.IsNullOrEmpty(path)
                ? null
                : UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path)?.name;

            return name != null && name.StartsWith(PackagePrefix, StringComparison.Ordinal) ? name : null;
        }

        /// <summary>
        ///     Maps a package name back to its project, e.g. com.sagittaras.gamedevkit.guard-clauses to Sagittaras.GuardClauses.
        /// </summary>
        private static string ProjectName(string package)
        {
            string slug = package.Substring(PackagePrefix.Length);
            return "Sagittaras." + string.Concat(slug.Split('-').Select(part => char.ToUpperInvariant(part[0]) + part.Substring(1)));
        }

        private static void BuildProject(string project)
        {
            string projectFile = Path.Combine(project, $"{project}.csproj");
            if (!File.Exists(Path.Combine(CSharpDirectory, projectFile)))
            {
                Debug.LogError($"Project {project} does not exist at srcs/csharp/{projectFile}.");
                return;
            }

            Build(projectFile, project);
        }

        private static async void Build(string target, string label)
        {
            _isBuilding = true;
            int progress = Progress.Start($"Pre-image build of {label}", options: Progress.Options.Indefinite);
            StringBuilder output = new();

            try
            {
                using Process process = new()
                {
                    StartInfo = new ProcessStartInfo("dotnet", $"build \"{target}\" -c Release -p:UpdateUnityPackage=true")
                    {
                        WorkingDirectory = CSharpDirectory,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    },
                };
                process.OutputDataReceived += (_, e) => Append(output, e.Data);
                process.ErrorDataReceived += (_, e) => Append(output, e.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await Task.Run(process.WaitForExit);

                if (process.ExitCode == 0)
                {
                    Progress.Finish(progress);
                    Debug.Log($"Pre-image build of {label} succeeded.\n{Lines(output, " pre-image ")}");
                    AssetDatabase.Refresh();
                }
                else
                {
                    Progress.Finish(progress, Progress.Status.Failed);
                    string errors = Lines(output, ": error ");
                    Debug.LogError($"Pre-image build of {label} failed (exit code {process.ExitCode}).\n{(errors.Length > 0 ? errors : output.ToString())}");
                }
            }
            catch (Win32Exception)
            {
                Progress.Finish(progress, Progress.Status.Failed);
                Debug.LogError("Pre-image build needs the .NET SDK — 'dotnet' was not found on the PATH Unity was started with.");
            }
            finally
            {
                _isBuilding = false;
            }
        }

        private static void Append(StringBuilder output, string? line)
        {
            if (line == null)
            {
                return;
            }

            lock (output)
            {
                output.AppendLine(line);
            }
        }

        /// <summary>
        ///     Lines of the build output containing the marker, deduplicated — MSBuild repeats errors in its summary.
        /// </summary>
        private static string Lines(StringBuilder output, string marker)
        {
            lock (output)
            {
                return string.Join("\n", output.ToString()
                    .Split('\n')
                    .Select(line => line.Trim())
                    .Where(line => line.Contains(marker))
                    .Distinct());
            }
        }
    }
}

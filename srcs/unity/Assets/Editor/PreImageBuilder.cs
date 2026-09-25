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
    internal static class PreImageBuilder
    {
        private const string PackagePrefix = "com.sagittaras.gamedevkit.";
        private const string BuildAllMenu = "Tools/Sagittaras/Build All Pre-images";
        private const string BuildPackageMenu = "Assets/Sagittaras/Build Package Pre-image";

        /// <summary>
        ///     Whether a build is running — only one at a time, both builds write into the same Plugins/ folders.
        /// </summary>
        private static bool _isBuilding;

        /// <summary>
        ///     The srcs/csharp folder, a sibling of this Unity project.
        /// </summary>
        private static string CSharpDirectory => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "csharp"));

        [MenuItem(BuildAllMenu)]
        private static void BuildAll()
        {
            Build("GameDevKit.sln", "all packages");
        }

        [MenuItem(BuildAllMenu, true)]
        private static bool CanBuildAll()
        {
            return !_isBuilding;
        }

        [MenuItem(BuildPackageMenu)]
        private static void BuildSelectedPackage()
        {
            string package = SelectedPackage() ?? throw new InvalidOperationException("No kit package is selected.");
            string project = ProjectName(package);
            string projectFile = Path.Combine(project, $"{project}.csproj");

            if (!File.Exists(Path.Combine(CSharpDirectory, projectFile)))
            {
                Debug.LogError($"Package {package} has no project at srcs/csharp/{projectFile}.");
                return;
            }

            Build(projectFile, package);
        }

        [MenuItem(BuildPackageMenu, true)]
        private static bool CanBuildSelectedPackage()
        {
            return !_isBuilding && SelectedPackage() != null;
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

        private static async void Build(string target, string label)
        {
            _isBuilding = true;
            int progress = Progress.Start($"Pre-image build of {label}", options: Progress.Options.Indefinite);
            StringBuilder output = new();

            try
            {
                using Process process = new();
                process.StartInfo = new ProcessStartInfo("dotnet", $"build \"{target}\" -c Release -p:UpdateUnityPackage=true")
                {
                    WorkingDirectory = CSharpDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
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

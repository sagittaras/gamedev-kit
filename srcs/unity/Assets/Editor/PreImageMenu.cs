#nullable enable
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sagittaras.DevelopmentKit.Editor
{
    /// <summary>
    ///     Menu items refreshing the Unity package pre-images without leaving Unity — the whole kit or a single package,
    ///     from the Tools menu or the Project window's context menu. The build itself is a <see cref="PreImageBuild"/>;
    ///     this class runs one at a time, reports its result and reimports the refreshed Plugins/ folders.
    /// </summary>
    /// <remarks>
    ///     Unity has no public API for menu items created at runtime, so every package has its own item below.
    ///     A package skeleton without one is reported on every domain reload.
    /// </remarks>
    internal static class PreImageMenu
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
        ///     The running build — only one at a time, every build writes into the same Plugins/ folders.
        /// </summary>
        private static PreImageBuild? _running;

        [MenuItem(BuildAllMenu, priority = AllPriority)]
        private static void BuildAll()
        {
            Run(PreImageBuild.ForSolution());
        }

        [MenuItem(BuildMenu + "Sagittaras.Dices", priority = PackagePriority)]
        private static void BuildDices()
        {
            Run(PreImageBuild.ForProject("Sagittaras.Dices"));
        }

        [MenuItem(BuildMenu + "Sagittaras.GuardClauses", priority = PackagePriority)]
        private static void BuildGuardClauses()
        {
            Run(PreImageBuild.ForProject("Sagittaras.GuardClauses"));
        }

        [MenuItem(BuildAllMenu, true)]
        [MenuItem(BuildMenu + "Sagittaras.Dices", true)]
        [MenuItem(BuildMenu + "Sagittaras.GuardClauses", true)]
        private static bool CanBuild()
        {
            return _running == null;
        }

        [MenuItem(PackageContextMenu)]
        private static void BuildSelectedPackage()
        {
            string package = SelectedPackage() ?? throw new InvalidOperationException("No kit package is selected.");
            Run(PreImageBuild.ForProject(ProjectName(package)));
        }

        [MenuItem(PackageContextMenu, true)]
        private static bool CanBuildSelectedPackage()
        {
            return _running == null && SelectedPackage() != null;
        }

        /// <summary>
        ///     Reports package skeletons that have no item in the Build Pre-image menu yet.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void ReportPackagesWithoutMenuItem()
        {
            string[] listed = TypeCache.GetMethodsWithAttribute<MenuItem>()
                .Where(method => method.DeclaringType == typeof(PreImageMenu))
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
                    Debug.LogWarning($"Package {Path.GetFileName(skeleton)} has no item in {BuildMenu.TrimEnd('/')} — add {project} to {nameof(PreImageMenu)}.");
                }
            }
        }

        /// <summary>
        ///     Runs the build to its end. Menu items cannot await, so this is where the build's task is observed:
        ///     auto refresh is held off while dotnet writes into Plugins/, so no reimport or domain reload cuts into
        ///     the build, and the refreshed pre-images are reimported once it succeeds.
        /// </summary>
        private static async void Run(PreImageBuild build)
        {
            _running = build;
            PreImageBuildResult result;
            AssetDatabase.DisallowAutoRefresh();

            try
            {
                result = await build.RunAsync();
            }
            finally
            {
                AssetDatabase.AllowAutoRefresh();
                _running = null;
            }

            Report(result);
            if (result.Status == PreImageBuildStatus.Succeeded)
            {
                AssetDatabase.Refresh();
            }
        }

        private static void Report(PreImageBuildResult result)
        {
            string summary = $"Pre-image build of {result.Label}";
            string duration = $"{result.Duration.TotalSeconds:0.0} s";

            switch (result.Status)
            {
                case PreImageBuildStatus.Succeeded:
                    Debug.Log($"{summary} succeeded in {duration}.\n{string.Join("\n", result.PreImages)}");
                    break;
                case PreImageBuildStatus.Cancelled:
                    Debug.LogWarning($"{summary} was cancelled after {duration}; pre-images may be partially refreshed.");
                    break;
                default:
                    string details = string.Join("\n", result.Errors.Any() ? result.Errors : result.Output.TakeLast(20));
                    string exitCode = result.ExitCode.HasValue ? $" with exit code {result.ExitCode}" : "";
                    Debug.LogError($"{summary} failed{exitCode} after {duration}.\n{details}");
                    break;
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
    }
}

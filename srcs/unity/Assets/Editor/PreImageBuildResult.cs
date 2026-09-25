#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sagittaras.DevelopmentKit.Editor
{
    /// <summary>
    ///     Outcome of a finished <see cref="PreImageBuild"/>.
    /// </summary>
    internal sealed class PreImageBuildResult
    {
        /// <summary>
        ///     Creates the outcome of a finished build.
        /// </summary>
        /// <param name="label">What was built, as shown to the user.</param>
        /// <param name="status">How the build ended.</param>
        /// <param name="exitCode">Exit code of dotnet, or <c>null</c> when it did not start.</param>
        /// <param name="output">Trimmed lines of the build output.</param>
        /// <param name="duration">How long the build ran.</param>
        public PreImageBuildResult(string label, PreImageBuildStatus status, int? exitCode, IReadOnlyList<string> output, TimeSpan duration)
        {
            Label = label;
            Status = status;
            ExitCode = exitCode;
            Output = output;
            Duration = duration;
        }

        /// <summary>
        ///     What was built, as shown to the user.
        /// </summary>
        public string Label { get; }

        /// <summary>
        ///     How the build ended.
        /// </summary>
        public PreImageBuildStatus Status { get; }

        /// <summary>
        ///     Exit code of dotnet, or <c>null</c> when it did not start.
        /// </summary>
        public int? ExitCode { get; }

        /// <summary>
        ///     Trimmed lines of the build output.
        /// </summary>
        public IReadOnlyList<string> Output { get; }

        /// <summary>
        ///     How long the build ran.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        ///     What the build did with each pre-image — refreshed it, or left it untouched at its version.
        /// </summary>
        public IEnumerable<string> PreImages => Output.Where(line => line.Contains(" pre-image ")).Distinct();

        /// <summary>
        ///     Errors reported by the build, deduplicated — MSBuild repeats them in its summary.
        /// </summary>
        public IEnumerable<string> Errors => Output.Where(line => line.Contains(": error ")).Distinct();
    }
}

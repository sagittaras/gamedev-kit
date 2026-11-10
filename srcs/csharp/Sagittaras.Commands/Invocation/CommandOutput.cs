using System;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Commands.Invocation
{
    /// <summary>
    ///     The plain result of an invocation - an exit code and what there is to read about it.
    /// </summary>
    /// <remarks>
    ///     Enough for a command whose answer is text. One that answers with more than that - a table of
    ///     items, a tree of entities - derives its own output carrying the payload, and whoever receives it
    ///     decides by the type how to show it, while <see cref="Content"/> stays the plain reading of the same
    ///     answer for anybody who does not know the type. The package answers this way itself, with
    ///     <see cref="UsageOutput"/> for a branch named alone and <c>RefusalOutput</c> for a line it could not
    ///     read.
    /// </remarks>
    public class CommandOutput : ICommandOutput
    {
        /// <summary>
        ///     Creates an output of the given exit code.
        /// </summary>
        /// <param name="exitCode">Exit code of the invocation, zero for success.</param>
        /// <param name="content">Content of the invocation.</param>
        protected CommandOutput(int exitCode, string content)
        {
            ExitCode = exitCode;
            Content = content;
        }

        /// <inheritdoc />
        public int ExitCode { get; }

        /// <inheritdoc />
        public string Content { get; }

        /// <summary>
        ///     Creates an output of a command that did its work.
        /// </summary>
        /// <param name="content">Content of the invocation, empty when the work speaks for itself.</param>
        /// <returns>Returns the output of a successful invocation.</returns>
        public static CommandOutput Success(string content = "")
        {
            return new CommandOutput(0, content);
        }

        /// <summary>
        ///     Creates an output of a command that could not do its work.
        /// </summary>
        /// <remarks>
        ///     For what the command itself ran into - a missing entity, a scene that is not loaded. What the
        ///     input was wrong about never reaches an invocation.
        /// </remarks>
        /// <param name="content">Reading of what went wrong.</param>
        /// <param name="exitCode">Exit code of the invocation, telling the failures apart where it matters.</param>
        /// <returns>Returns the output of a failed invocation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the exit code is zero, which is what a success is told by.</exception>
        public static CommandOutput Failure(string content, int exitCode = 1)
        {
            Guard.Against.Zero(exitCode, "A failure needs a non-zero exit code - zero is what a success is told by.");

            return new CommandOutput(exitCode, content);
        }
    }
}

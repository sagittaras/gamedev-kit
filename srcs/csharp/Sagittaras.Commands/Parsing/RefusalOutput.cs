using System;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parsing.Errors;

namespace Sagittaras.Commands.Parsing
{
    /// <summary>
    ///     The answer to a line the parser could not make sense of.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Carries the whole result of the parsing rather than only its message, so a console can point at
    ///         the word the error names through <see cref="IParseError.Token"/>, colour the rest of the line by
    ///         <see cref="IParseResult.Tokens"/>, or answer with the usage of <see cref="IParseResult.Command"/>
    ///         when the line got as far as naming one - straight from <c>Execute</c>, without parsing the line a
    ///         second time. <see cref="CommandOutput.Content"/> is the message of the error, for a console that
    ///         does none of that.
    ///     </para>
    /// </remarks>
    public class RefusalOutput : CommandOutput
    {
        /// <summary>
        ///     Exit code of a line the parser could not make sense of.
        /// </summary>
        /// <remarks>
        ///     Told apart from the exit code a failing command answers with, so a console can react to a mistyped
        ///     line differently than to work that did not go through.
        /// </remarks>
        public const int UsageExitCode = 2;

        /// <summary>
        ///     Creates the answer to a failed parsing.
        /// </summary>
        /// <param name="result">Result of the parsing that failed.</param>
        /// <exception cref="ArgumentException">Thrown when the parsing did not fail.</exception>
        public RefusalOutput(IParseResult result)
            : base(UsageExitCode, RequireFailed(result).Error!.Message)
        {
            Result = result;
        }

        /// <summary>
        ///     Result of the parsing that failed.
        /// </summary>
        public IParseResult Result { get; }

        /// <summary>
        ///     Reason the parsing failed.
        /// </summary>
        public IParseError Error => Result.Error!;

        /// <summary>
        ///     Ensures the result is one there is something to refuse about.
        /// </summary>
        /// <param name="result">Result to guard.</param>
        /// <returns>Returns the guarded result.</returns>
        /// <exception cref="ArgumentException">Thrown when the parsing did not fail.</exception>
        private static IParseResult RequireFailed(IParseResult result)
        {
            return result.Success
                ? throw new ArgumentException("A line the parser made sense of is nothing to refuse - it is to be run.", nameof(result))
                : result;
        }
    }
}

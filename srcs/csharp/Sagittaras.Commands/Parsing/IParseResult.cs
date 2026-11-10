using System.Collections.Generic;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parsing.Errors;
using Sagittaras.Commands.Parsing.Tokens;

namespace Sagittaras.Commands.Parsing
{
    /// <summary>
    ///     Represents the result of parsing the input into <see cref="ICommand"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Everything the parser has to say, and the seam the invocation is separated by: what the
    ///         command is given is <see cref="Input"/> alone, while the tokens and the error stay here, with
    ///         the console that has a use for them.
    ///     </para>
    ///     <para>
    ///         Parsing stops at the first thing it cannot make sense of, so there is at most one
    ///         <see cref="Error"/> - reporting the second problem of an input whose first word was already
    ///         wrong would be guessing.
    ///     </para>
    /// </remarks>
    public interface IParseResult
    {
        /// <summary>
        ///     Indicates whether the parsing was successful.
        /// </summary>
        /// <remarks>
        ///     True when the input resolved to a command and every value it needs was bound, which is
        ///     exactly when <see cref="Command"/> and <see cref="Input"/> are both there.
        /// </remarks>
        bool Success { get; }

        /// <summary>
        ///     Instance of resolved <see cref="ICommand"/>.
        /// </summary>
        /// <remarks>
        ///     Present even on an unsuccessful parsing whenever the input got far enough to name a command -
        ///     which is what lets the console answer a mistyped value with the usage of the command it was
        ///     meant for.
        /// </remarks>
        ICommand? Command { get; }

        /// <summary>
        ///     Values bound for the resolved command.
        /// </summary>
        /// <remarks>
        ///     Null until the binding has run through, so an unsuccessful parsing may leave it behind even
        ///     with a <see cref="Command"/> already resolved.
        /// </remarks>
        ICommandInput? Input { get; }

        /// <summary>
        ///     Reason the parsing failed.
        /// </summary>
        /// <remarks>
        ///     Null on a successful parsing.
        /// </remarks>
        IParseError? Error { get; }

        /// <summary>
        ///     Tokens the input was cut into, in the order they appear in it.
        /// </summary>
        /// <remarks>
        ///     Carries the whole input, the unmatched tokens included, each with the role it was given and
        ///     the place it holds - which is what a console highlighting the line reads.
        /// </remarks>
        IReadOnlyList<Token> Tokens { get; }

        /// <summary>
        ///     Tokens that were left without a role.
        /// </summary>
        /// <remarks>
        ///     Contains tokens not accounted for by any command, argument or option. A subset of
        ///     <see cref="Tokens"/>, offered apart so that answering the mistake does not mean walking the
        ///     whole input again.
        /// </remarks>
        IReadOnlyList<Token> UnmatchedTokens { get; }
    }
}

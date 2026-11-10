using System.Collections.Generic;
using System.Linq;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parsing.Errors;
using Sagittaras.Commands.Parsing.Tokens;

namespace Sagittaras.Commands.Parsing
{
    /// <summary>
    ///     What the parser made of an input.
    /// </summary>
    public class ParseResult : IParseResult
    {
        /// <summary>
        ///     Creates a result of the given parts.
        /// </summary>
        /// <param name="command">Command the input resolved to, if it got that far.</param>
        /// <param name="input">Values bound for the command, if the binding ran through.</param>
        /// <param name="error">Reason the parsing failed, or null when it did not.</param>
        /// <param name="tokens">Tokens the input was cut into.</param>
        private ParseResult(ICommand? command, ICommandInput? input, IParseError? error, IReadOnlyList<Token> tokens)
        {
            Command = command;
            Input = input;
            Error = error;
            Tokens = tokens;
            UnmatchedTokens = CollectUnmatched(tokens);
        }

        /// <inheritdoc />
        public bool Success => Error == null;

        /// <inheritdoc />
        public ICommand? Command { get; }

        /// <inheritdoc />
        public ICommandInput? Input { get; }

        /// <inheritdoc />
        public IParseError? Error { get; }

        /// <inheritdoc />
        public IReadOnlyList<Token> Tokens { get; }

        /// <inheritdoc />
        public IReadOnlyList<Token> UnmatchedTokens { get; }

        /// <summary>
        ///     Reports an input the parser made sense of all the way through.
        /// </summary>
        /// <param name="command">Command the input resolved to.</param>
        /// <param name="input">Values bound for the command.</param>
        /// <param name="tokens">Tokens the input was cut into, each with the role it was given.</param>
        /// <returns>Returns the result of a successful parsing.</returns>
        public static ParseResult Parsed(ICommand command, ICommandInput input, IReadOnlyList<Token> tokens)
        {
            return new ParseResult(command, input, null, tokens);
        }

        /// <summary>
        ///     Reports an input the parser could not make sense of.
        /// </summary>
        /// <remarks>
        ///     The command is carried along whenever the input got far enough to name one, so that the answer
        ///     can be about that command rather than about the input in general.
        /// </remarks>
        /// <param name="error">Reason the parsing failed.</param>
        /// <param name="command">Command the input resolved to before it failed, if any.</param>
        /// <param name="tokens">Tokens the input was cut into, those left without a role included.</param>
        /// <returns>Returns the result of a failed parsing.</returns>
        public static ParseResult Failed(IParseError error, ICommand? command, IReadOnlyList<Token> tokens)
        {
            return new ParseResult(command, null, error, tokens);
        }

        /// <summary>
        ///     Picks the tokens that were left without a role.
        /// </summary>
        /// <param name="tokens">Tokens the input was cut into.</param>
        /// <returns>Returns the tokens of an unknown role, empty when every token was accounted for.</returns>
        private static IReadOnlyList<Token> CollectUnmatched(IReadOnlyList<Token> tokens)
        {
            return tokens.Where(token => token.Type == TokenType.Unknown).ToArray();
        }
    }
}

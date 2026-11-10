using Sagittaras.Commands.Parsing.Tokens;

namespace Sagittaras.Commands.Parsing.Errors
{
    /// <summary>
    ///     The plain reason the input could not be parsed.
    /// </summary>
    public class ParseError : IParseError
    {
        /// <summary>
        ///     Creates a reason of the given code.
        /// </summary>
        /// <param name="code">Reason the parsing failed.</param>
        /// <param name="message">Reading of the reason, meant for whoever typed the input.</param>
        /// <param name="token">Token the error is about, or null when the input is at fault by what it does not hold.</param>
        public ParseError(ParseErrorCode code, string message, Token? token = null)
        {
            Code = code;
            Message = message;
            Token = token;
        }

        /// <inheritdoc />
        public ParseErrorCode Code { get; }

        /// <inheritdoc />
        public string Message { get; }

        /// <inheritdoc />
        public Token? Token { get; }
    }
}

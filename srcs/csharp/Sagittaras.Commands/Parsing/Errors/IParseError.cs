using Sagittaras.Commands.Parsing.Tokens;

namespace Sagittaras.Commands.Parsing.Errors
{
    /// <summary>
    ///     Describes why the input could not be parsed.
    /// </summary>
    /// <remarks>
    ///     A typo is the most ordinary thing that happens to a console, so a failed parsing is reported
    ///     rather than thrown. Being a value and not an exception is also what keeps the reason usable: the
    ///     code can be answered differently than it is printed, and <see cref="Token"/> points at the very
    ///     word to say it about.
    /// </remarks>
    public interface IParseError
    {
        /// <summary>
        ///     Reason the parsing failed.
        /// </summary>
        ParseErrorCode Code { get; }

        /// <summary>
        ///     Reading of the reason, meant for whoever typed the input.
        /// </summary>
        string Message { get; }

        /// <summary>
        ///     Token the error is about.
        /// </summary>
        /// <remarks>
        ///     Null when the input is at fault by what it does not hold - an argument never given, an
        ///     option left without its value - as there is no word to point at in that case.
        /// </remarks>
        Token? Token { get; }
    }
}

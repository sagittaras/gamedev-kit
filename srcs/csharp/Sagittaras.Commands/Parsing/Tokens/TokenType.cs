namespace Sagittaras.Commands.Parsing.Tokens
{
    /// <summary>
    ///     Role a <see cref="Token"/> plays in the input it was cut from.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        ///     Indicates that the token could not be given a role.
        /// </summary>
        /// <remarks>
        ///     The state a token is left in when nothing on the command it belongs to accounts for it,
        ///     which is how it ends up in <see cref="IParseResult.UnmatchedTokens"/>.
        /// </remarks>
        Unknown,

        /// <summary>
        ///     Indicates that the token represents a command's name.
        /// </summary>
        Command,

        /// <summary>
        ///     Indicates that the token carries the value of a positional argument.
        /// </summary>
        Argument,

        /// <summary>
        ///     Indicates that the token names an option.
        /// </summary>
        Option,

        /// <summary>
        ///     Indicates that the token carries the value of the option named before it.
        /// </summary>
        /// <remarks>
        ///     A flag has no such token - being named is the whole of its value.
        /// </remarks>
        OptionValue,
    }
}

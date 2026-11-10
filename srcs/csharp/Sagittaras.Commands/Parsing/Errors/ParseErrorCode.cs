namespace Sagittaras.Commands.Parsing.Errors
{
    /// <summary>
    ///     Names the reason the input could not be parsed.
    /// </summary>
    /// <remarks>
    ///     The members follow the order the parser works in - cutting the input, walking the hierarchy,
    ///     binding the values - so the reason also says how far the parsing got before it gave up.
    /// </remarks>
    public enum ParseErrorCode
    {
        /// <summary>
        ///     The input held nothing to parse.
        /// </summary>
        EmptyInput,

        /// <summary>
        ///     The input opened a quote it never closed.
        /// </summary>
        UnterminatedQuote,

        /// <summary>
        ///     The first token of the input names no registered command.
        /// </summary>
        UnknownCommand,

        /// <summary>
        ///     A command was reached, but the token behind it names none of its subcommands.
        /// </summary>
        UnknownSubcommand,

        /// <summary>
        ///     The input ended before a required argument was given.
        /// </summary>
        MissingArgument,

        /// <summary>
        ///     The input held more values than the command has arguments to bind them to.
        /// </summary>
        TooManyArguments,

        /// <summary>
        ///     The input named an option the command does not declare.
        /// </summary>
        UnknownOption,

        /// <summary>
        ///     An option was named, but the value it takes never followed.
        /// </summary>
        MissingOptionValue,

        /// <summary>
        ///     A value could not be converted to the type its parameter declares.
        /// </summary>
        ValueTypeMismatch,

        /// <summary>
        ///     The own parser of a parameter refused the value.
        /// </summary>
        /// <remarks>
        ///     Told apart from <see cref="ValueTypeMismatch"/> on purpose: the value may well be of the right
        ///     type and still break a rule of the parameter - a share over a hundred, an entity not in the
        ///     scene - and the message is the parser's own.
        /// </remarks>
        ValueRefused,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parameters;
using Sagittaras.Commands.Parsing.Conversion;
using Sagittaras.Commands.Parsing.Errors;
using Sagittaras.Commands.Parsing.Tokens;

namespace Sagittaras.Commands.Parsing
{
    /// <summary>
    ///     Reads a console line in three steps: cutting it up, walking the hierarchy, binding the values.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The walk comes first and stops at the first token that is not the name of a subcommand, so
    ///         the path is spelled out before anything else - <c>spawn entity goblin --scene arena</c>. An
    ///         option met during the walk ends it, which is what keeps the parser from having to know whose
    ///         option it is before it knows which command was meant.
    ///     </para>
    ///     <para>
    ///         Names, aliases and enum members are all matched ignoring the case, the way a line typed into
    ///         a console has to be. Everything else about reading a value is left to
    ///         <see cref="IValueConverter"/>.
    ///     </para>
    /// </remarks>
    public class CommandParser : ICommandParser
    {
        /// <summary>
        ///     Reader turning a piece of input into the value a parameter holds.
        /// </summary>
        private readonly IValueConverter _converter;

        /// <summary>
        ///     Creates a parser reading the types a console line is usually spelled in.
        /// </summary>
        public CommandParser()
            : this(new DefaultValueConverter())
        {
        }

        /// <summary>
        ///     Creates a parser reading the values through the given converter.
        /// </summary>
        /// <param name="converter">Converter turning a piece of input into the value a parameter holds.</param>
        public CommandParser(IValueConverter converter)
        {
            _converter = converter;
        }

        /// <inheritdoc />
        public IParseResult Parse(string input, IReadOnlyList<ICommand> commands)
        {
            List<Token> tokens = new();

            if (!Tokenizer.TryTokenize(input, tokens, out IParseError? error))
            {
                return ParseResult.Failed(error!, null, tokens);
            }

            if (tokens.Count == 0)
            {
                return ParseResult.Failed(new ParseError(ParseErrorCode.EmptyInput, "The input holds nothing to run."), null, tokens);
            }

            int index = 0;
            List<ICommand> chain = new();
            ICommand? command = Resolve(commands, tokens, ref index, chain);
            string path = Spell(chain);

            if (command == null)
            {
                return ParseResult.Failed(new ParseError(ParseErrorCode.UnknownCommand, $"There is no command named '{tokens[0].Lexeme}'.", tokens[0]), null, tokens);
            }

            if (command.Subcommands.Count > 0 && index < tokens.Count && !IsNamed(tokens[index]))
            {
                return ParseResult.Failed(new ParseError(ParseErrorCode.UnknownSubcommand, $"Command '{path}' holds no subcommand named '{tokens[index].Lexeme}'.", tokens[index]), command, tokens);
            }

            return Bind(command, path, tokens, index);
        }

        /// <summary>
        ///     Walks the hierarchy for as long as the input keeps naming commands.
        /// </summary>
        /// <param name="commands">Commands the walk starts from.</param>
        /// <param name="tokens">Tokens the input was cut into, stamped as commands on the way.</param>
        /// <param name="index">Index to walk from, left behind the last command that was named.</param>
        /// <param name="chain">Commands the walk went through, filled on the way.</param>
        /// <returns>Returns the command the input arrived at, or null when the first token names none.</returns>
        private static ICommand? Resolve(IReadOnlyList<ICommand> commands, List<Token> tokens, ref int index, List<ICommand> chain)
        {
            ICommand? current = null;
            IReadOnlyList<ICommand> candidates = commands;

            while (index < tokens.Count && !IsNamed(tokens[index]))
            {
                ICommand? match = Match(candidates, tokens[index].Lexeme);

                if (match == null)
                {
                    break;
                }

                tokens[index] = Stamp(tokens[index], TokenType.Command);
                index++;
                current = match;
                chain.Add(match);
                candidates = match.Subcommands;
            }

            return current;
        }

        /// <summary>
        ///     Binds what is left of the input to the parameters of the command.
        /// </summary>
        /// <param name="command">Command the input resolved to.</param>
        /// <param name="path">Names of the commands the input walked through, the way the errors spell the command.</param>
        /// <param name="tokens">Tokens the input was cut into, stamped with their roles on the way.</param>
        /// <param name="index">Index to bind from.</param>
        /// <returns>Returns the result of the binding, successful or not.</returns>
        private IParseResult Bind(ICommand command, string path, List<Token> tokens, int index)
        {
            Dictionary<IParameter, BoundValue> values = new();
            int positional = 0;

            while (index < tokens.Count)
            {
                IParseError? error = IsNamed(tokens[index])
                    ? BindOption(command, path, tokens, ref index, values)
                    : BindArgument(command, path, tokens, ref index, ref positional, values);

                if (error != null)
                {
                    return ParseResult.Failed(error, command, tokens);
                }
            }

            for (int remaining = positional; remaining < command.Arguments.Count; remaining++)
            {
                IArgument argument = command.Arguments[remaining];

                if (!argument.IsOptional)
                {
                    return ParseResult.Failed(new ParseError(ParseErrorCode.MissingArgument, $"Command '{path}' needs the argument '{argument.Name}', which the input never reaches."), command, tokens);
                }

                values[argument] = BoundValue.Defaulted(argument.DefaultValue);
            }

            foreach (IOption option in command.Options)
            {
                if (!values.ContainsKey(option))
                {
                    values[option] = BoundValue.Defaulted(option.DefaultValue);
                }
            }

            return ParseResult.Parsed(command, new CommandInput(values), tokens);
        }

        /// <summary>
        ///     Binds an option the input named, together with the value it takes.
        /// </summary>
        /// <param name="command">Command the input resolved to.</param>
        /// <param name="path">Names of the commands the input walked through, the way the errors spell the command.</param>
        /// <param name="tokens">Tokens the input was cut into.</param>
        /// <param name="index">Index of the option token, left behind what was bound.</param>
        /// <param name="values">Values bound so far.</param>
        /// <returns>Returns the reason the option could not be bound, or null when it was.</returns>
        private IParseError? BindOption(ICommand command, string path, List<Token> tokens, ref int index, Dictionary<IParameter, BoundValue> values)
        {
            Token named = tokens[index];
            IOption? option = MatchOption(command.Options, named.Lexeme.TrimStart('-'));

            if (option == null)
            {
                return new ParseError(ParseErrorCode.UnknownOption, $"Command '{path}' knows no option named '{named.Lexeme}'.", named);
            }

            tokens[index] = Stamp(named, TokenType.Option);
            index++;

            bool paired = index < tokens.Count && tokens[index].Type == TokenType.OptionValue;

            if (option.IsFlag && !paired)
            {
                values[option] = BoundValue.Given(true);

                return null;
            }

            if (!paired && (index >= tokens.Count || IsNamed(tokens[index])))
            {
                return new ParseError(ParseErrorCode.MissingOptionValue, $"Option '{option.Name}' takes a value, which never follows it.", tokens[index - 1]);
            }

            Token value = tokens[index];

            IParseError? unread = Read(option, "Option", value, out object? converted);

            if (unread != null)
            {
                return unread;
            }

            tokens[index] = Stamp(value, TokenType.OptionValue);
            index++;
            values[option] = BoundValue.Given(converted);

            return null;
        }

        /// <summary>
        ///     Binds a value to the argument whose place in the input it holds.
        /// </summary>
        /// <param name="command">Command the input resolved to.</param>
        /// <param name="path">Names of the commands the input walked through, the way the errors spell the command.</param>
        /// <param name="tokens">Tokens the input was cut into.</param>
        /// <param name="index">Index of the value token, left behind it.</param>
        /// <param name="positional">Number of arguments bound so far, raised by the one that was bound.</param>
        /// <param name="values">Values bound so far.</param>
        /// <returns>Returns the reason the value could not be bound, or null when it was.</returns>
        private IParseError? BindArgument(ICommand command, string path, List<Token> tokens, ref int index, ref int positional, Dictionary<IParameter, BoundValue> values)
        {
            Token value = tokens[index];

            if (command.Subcommands.Count > 0)
            {
                return Match(command.Subcommands, value.Lexeme) != null
                    ? new ParseError(ParseErrorCode.UnknownSubcommand, $"Subcommand '{value.Lexeme}' has to be named before the options of '{path}'.", value)
                    : new ParseError(ParseErrorCode.UnknownSubcommand, $"Command '{path}' holds no subcommand named '{value.Lexeme}'.", value);
            }

            if (positional >= command.Arguments.Count)
            {
                return new ParseError(ParseErrorCode.TooManyArguments, $"Command '{path}' takes {command.Arguments.Count} argument(s), so there is nothing left for '{value.Lexeme}' to be.", value);
            }

            IArgument argument = command.Arguments[positional];

            IParseError? unread = Read(argument, "Argument", value, out object? converted);

            if (unread != null)
            {
                return unread;
            }

            tokens[index] = Stamp(value, TokenType.Argument);
            index++;
            positional++;
            values[argument] = BoundValue.Given(converted);

            return null;
        }

        /// <summary>
        ///     Reads the value of a token the way the parameter asks for, and has it judged.
        /// </summary>
        /// <param name="parameter">Parameter the value is read for.</param>
        /// <param name="kind">How the parameter is spoken of in the error - an argument or an option.</param>
        /// <param name="value">Token carrying the value.</param>
        /// <param name="converted">Value that was read, or null when it was not.</param>
        /// <returns>Returns the reason the value could not be taken, or null when it was.</returns>
        private IParseError? Read(IParameter parameter, string kind, Token value, out object? converted)
        {
            return Interpret(parameter, kind, value, out converted) ?? Validate(parameter, kind, value, converted);
        }

        /// <summary>
        ///     Turns the text of a token into the value of the parameter.
        /// </summary>
        /// <remarks>
        ///     The own parser of the parameter comes first and replaces the converter entirely, so a rule of the
        ///     parameter is never second-guessed by the reading of its type.
        /// </remarks>
        /// <param name="parameter">Parameter the value is read for.</param>
        /// <param name="kind">How the parameter is spoken of in the error - an argument or an option.</param>
        /// <param name="value">Token carrying the value.</param>
        /// <param name="converted">Value that was read, or null when it was not.</param>
        /// <returns>Returns the reason the text could not be read, or null when it was.</returns>
        private IParseError? Interpret(IParameter parameter, string kind, Token value, out object? converted)
        {
            Func<string, ParsedValue<object?>>? parser = parameter.Parser;

            if (parser == null)
            {
                return _converter.TryConvert(value.Lexeme, parameter.ValueType, out converted)
                    ? null
                    : new ParseError(ParseErrorCode.ValueTypeMismatch, $"{kind} '{parameter.Name}' takes {Describe(parameter.ValueType)}, which '{value.Lexeme}' is not.", value);
            }

            ParsedValue<object?> parsed = parser(value.Lexeme);
            converted = parsed.Value;

            return parsed.IsRead ? null : Refuse(parameter, kind, value, parsed.Message);
        }

        /// <summary>
        ///     Has a value that was read judged by the validator of the parameter.
        /// </summary>
        /// <remarks>
        ///     Reached only once the value was read, so the validator always judges a value of the declared type,
        ///     never the text it came from.
        /// </remarks>
        /// <param name="parameter">Parameter the value was read for.</param>
        /// <param name="kind">How the parameter is spoken of in the error - an argument or an option.</param>
        /// <param name="value">Token carrying the value.</param>
        /// <param name="converted">Value that was read.</param>
        /// <returns>Returns the reason the value was refused, or null when it was accepted.</returns>
        private static IParseError? Validate(IParameter parameter, string kind, Token value, object? converted)
        {
            string? refusal = parameter.Validator?.Invoke(converted);

            return refusal == null ? null : Refuse(parameter, kind, value, refusal);
        }

        /// <summary>
        ///     Reports a value the parameter refused, in its own words when it gave any.
        /// </summary>
        /// <param name="parameter">Parameter that refused the value.</param>
        /// <param name="kind">How the parameter is spoken of in the error - an argument or an option.</param>
        /// <param name="value">Token carrying the value.</param>
        /// <param name="reason">Reason the parameter gave, or null or blank when it gave none.</param>
        /// <returns>Returns the error pointing at the token.</returns>
        private static IParseError Refuse(IParameter parameter, string kind, Token value, string? reason)
        {
            return new ParseError(ParseErrorCode.ValueRefused, string.IsNullOrWhiteSpace(reason) ? $"{kind} '{parameter.Name}' does not accept '{value.Lexeme}'." : reason!, value);
        }

        /// <summary>
        ///     Tells whether the token carries the calling sign of an option.
        /// </summary>
        /// <param name="token">Token to ask about.</param>
        /// <returns>Returns true when the token names an option; otherwise false.</returns>
        private static bool IsNamed(Token token)
        {
            return token.Type == TokenType.Option || (token.Type == TokenType.Unknown && Tokenizer.LooksNamed(token.Lexeme));
        }

        /// <summary>
        ///     Finds the command the token names among the candidates.
        /// </summary>
        /// <param name="candidates">Commands to look through.</param>
        /// <param name="lexeme">Name the input spelled.</param>
        /// <returns>Returns the command of that name, or null when none of them answers to it.</returns>
        private static ICommand? Match(IReadOnlyList<ICommand> candidates, string lexeme)
        {
            return candidates.FirstOrDefault(candidate => string.Equals(candidate.Name, lexeme, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Finds the option the input named, by its name or any of its aliases.
        /// </summary>
        /// <param name="options">Options to look through.</param>
        /// <param name="name">Name the input spelled, without the prefix.</param>
        /// <returns>Returns the option answering to the name, or null when none does.</returns>
        private static IOption? MatchOption(IReadOnlyList<IOption> options, string name)
        {
            return options.FirstOrDefault(option => string.Equals(option.Name, name, StringComparison.OrdinalIgnoreCase)
                                                    || option.Aliases.Any(alias => string.Equals(alias, name, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        ///     Names the type a value was expected to be, for the error to read plainly.
        /// </summary>
        /// <param name="valueType">Type to name.</param>
        /// <returns>Returns the reading of the type.</returns>
        private static string Describe(Type valueType)
        {
            Type target = Nullable.GetUnderlyingType(valueType) ?? valueType;

            return target.IsEnum
                ? $"one of {string.Join(", ", Enum.GetNames(target))}"
                : $"a value of type {target.Name}";
        }

        /// <summary>
        ///     Spells the commands the walk went through, the way an error names the command it is about.
        /// </summary>
        /// <remarks>
        ///     The whole path rather than the last name, as a name is only unique among its siblings - an error
        ///     about <c>entity</c> could be about any command of that name in the hierarchy, while one about
        ///     <c>spawn entity</c> is about the one that was typed.
        /// </remarks>
        /// <param name="chain">Commands the walk went through.</param>
        /// <returns>Returns their names, separated the way the input separates them.</returns>
        private static string Spell(IReadOnlyList<ICommand> chain)
        {
            return string.Join(" ", chain.Select(command => command.Name));
        }

        /// <summary>
        ///     Copies the token with the role it turned out to play.
        /// </summary>
        /// <param name="token">Token to stamp.</param>
        /// <param name="type">Role the token plays in the input.</param>
        /// <returns>Returns the stamped token.</returns>
        private static Token Stamp(Token token, TokenType type)
        {
            return new Token(type, token.Lexeme, token.Start, token.Length);
        }
    }
}

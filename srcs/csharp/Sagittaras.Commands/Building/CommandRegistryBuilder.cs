using System;
using System.Collections.Generic;
using System.Linq;
using Sagittaras.Commands.Help;
using Sagittaras.Commands.Parameters;
using Sagittaras.Commands.Parsing;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Commands.Building
{
    /// <summary>
    ///     Collects the commands and hands out the manager serving them.
    /// </summary>
    public class CommandRegistryBuilder : ICommandRegistryBuilder
    {
        /// <summary>
        ///     Commands reachable by naming them first.
        /// </summary>
        private readonly List<ICommand> _commands = new();

        /// <summary>
        ///     Parser reading the lines of input, or null while the default one is to be used.
        /// </summary>
        private ICommandParser? _parser;

        /// <summary>
        ///     Whether the built-in help is added when the manager is built.
        /// </summary>
        private bool _withHelp;

        /// <inheritdoc />
        public ICommandRegistryBuilder Add(ICommand command)
        {
            Guard.Against.True(Holds(_commands, command.Name), $"A command named '{command.Name}' is already registered, and only one of the two could ever be reached.");
            Validate(command);
            _commands.Add(command);

            return this;
        }

        /// <inheritdoc />
        public ICommandRegistryBuilder WithParser(ICommandParser parser)
        {
            _parser = parser;

            return this;
        }

        /// <inheritdoc />
        public ICommandRegistryBuilder WithHelp()
        {
            _withHelp = true;

            return this;
        }

        /// <inheritdoc />
        public ICommandManager Build()
        {
            ICommand[] commands = _commands.ToArray();

            return new CommandManager(_withHelp ? WithHelpCommand(commands) : commands, _parser ?? new CommandParser());
        }

        /// <summary>
        ///     Adds the built-in help over the commands, as the last of them.
        /// </summary>
        /// <param name="commands">Commands the help describes.</param>
        /// <returns>Returns the commands with the help behind them.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a command already bears the name of the help.</exception>
        private static ICommand[] WithHelpCommand(ICommand[] commands)
        {
            Guard.Against.True(Holds(commands, HelpCommand.CommandName), $"A command named '{HelpCommand.CommandName}' is already registered, so the built-in help has no name left to answer to.");

            return commands.Append(new HelpCommand(commands)).ToArray();
        }

        /// <summary>
        ///     Checks over the command and everything below it.
        /// </summary>
        /// <remarks>
        ///     The bases of the package keep a command from being built wrongly in the first place, but
        ///     nothing stops an <see cref="ICommand"/> of somebody else's making from arriving here, and a
        ///     command the parser could resolve but nobody could run is worth hearing about now.
        /// </remarks>
        /// <param name="command">Command to check over.</param>
        /// <exception cref="InvalidOperationException">Thrown when the command or one below it could never be reached or run.</exception>
        private static void Validate(ICommand command)
        {
            Guard.Against.False(command is ISyncCommand or IAsyncCommand, $"Command '{command.Name}' implements neither ISyncCommand nor IAsyncCommand, so the parser could resolve it and nobody could run it.");
            Guard.Against.True(command.Subcommands.Count > 0 && command.Arguments.Count > 0, $"Command '{command.Name}' holds subcommands next to positional arguments - a token behind it would be both a subcommand name and the value of an argument.");
            RequireOptionalArgumentsLast(command);

            List<ICommand> siblings = new();

            foreach (ICommand subcommand in command.Subcommands)
            {
                Guard.Against.True(Holds(siblings, subcommand.Name), $"Command '{command.Name}' holds two subcommands named '{subcommand.Name}', and only one of them could ever be reached.");
                siblings.Add(subcommand);
                Validate(subcommand);
            }
        }

        /// <summary>
        ///     Ensures no required argument of the command stands behind an optional one.
        /// </summary>
        /// <remarks>
        ///     Values go to the arguments in the order they are declared, and the parser cannot know the line
        ///     meant to leave an optional argument out until it runs out of values. An optional argument in front
        ///     of a required one would therefore always take the value meant for the required one, and the
        ///     required one would be reported missing on a line that held everything the command needs.
        /// </remarks>
        /// <param name="command">Command whose arguments are checked.</param>
        /// <exception cref="InvalidOperationException">Thrown when a required argument stands behind an optional one.</exception>
        private static void RequireOptionalArgumentsLast(ICommand command)
        {
            IArgument? optional = command.Arguments.FirstOrDefault(argument => argument.IsOptional);
            IArgument? misplaced = command.Arguments.SkipWhile(argument => !argument.IsOptional).FirstOrDefault(argument => !argument.IsOptional);

            Guard.Against.True(misplaced != null, $"Command '{command.Name}' declares the required argument '{misplaced?.Name}' behind the optional '{optional?.Name}' - values go to the arguments in the order they are declared, so the optional one would always take the value meant for the required one.");
        }

        /// <summary>
        ///     Tells whether the commands already hold one of the given name.
        /// </summary>
        /// <remarks>
        ///     Names are told apart the way the input is matched against them, which ignores the case.
        /// </remarks>
        /// <param name="commands">Commands to look through.</param>
        /// <param name="name">Name to look for.</param>
        /// <returns>Returns true when a command of that name is already held; otherwise false.</returns>
        private static bool Holds(IReadOnlyList<ICommand> commands, string name)
        {
            return commands.Any(command => string.Equals(command.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}

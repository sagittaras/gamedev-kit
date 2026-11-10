using System.Collections.Generic;
using Sagittaras.Commands.Invocation;

namespace Sagittaras.Commands.Help
{
    /// <summary>
    ///     The built-in help - the commands there are when named alone, everything about one when a path follows.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The path behind it can be as long as the hierarchy is deep, and positional arguments come in a
    ///         fixed number. So rather than taking the path as values, the help holds a mirror of the whole
    ///         hierarchy as its subcommands: <c>help spawn entity</c> is walked by the parser like any other
    ///         line, and gets everything the parser gives any other line - names matched ignoring the case, the
    ///         tokens stamped for highlighting, a mistyped name reported and pointed at.
    ///     </para>
    ///     <para>
    ///         Created by <see cref="Building.ICommandRegistryBuilder.WithHelp"/> when the manager is built, as it
    ///         has to know every command registered by then; the mirror is of those, and later registrations
    ///         never reach it - the same way they never reach the manager itself.
    ///     </para>
    /// </remarks>
    public sealed class HelpCommand : CommandGroup
    {
        /// <summary>
        ///     Name the built-in help answers to.
        /// </summary>
        public const string CommandName = "help";

        /// <summary>
        ///     Commands the overview lists, the help itself among them.
        /// </summary>
        private readonly IReadOnlyList<ICommand> _commands;

        /// <summary>
        ///     Creates the help over the given commands.
        /// </summary>
        /// <param name="commands">Commands reachable by naming them first, the help itself not among them.</param>
        internal HelpCommand(IReadOnlyList<ICommand> commands)
            : base(CommandName, "Describes the commands there are, or the one named behind it")
        {
            _commands = new List<ICommand>(commands) { this };

            foreach (ICommand command in commands)
            {
                Add(new HelpTopic(new[] { command }, true));
            }

            Add(new HelpTopic(new ICommand[] { this }, false));
        }

        /// <summary>
        ///     Answers the help named alone with the overview of the commands there are.
        /// </summary>
        /// <param name="input">Values bound for this invocation.</param>
        /// <returns>Returns the overview.</returns>
        public override ICommandOutput Invoke(ICommandInput input)
        {
            return HelpOutput.Overview(_commands);
        }
    }
}

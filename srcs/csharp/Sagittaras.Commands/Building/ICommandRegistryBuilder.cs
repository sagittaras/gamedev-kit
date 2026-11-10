using System;
using Sagittaras.Commands.Parsing;

namespace Sagittaras.Commands.Building
{
    /// <summary>
    ///     Collects the commands a console offers and builds the <see cref="ICommandManager"/> serving them.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Registration is deliberately the expensive half. Whatever can be said about a command before
    ///         anybody types anything - that it can be run at all, that it does not hold subcommands and
    ///         positional arguments at once, that its optional arguments come last, that no two siblings answer
    ///         to the same name - is decided here, once during the boot, so that a console line is only ever
    ///         matched.
    ///     </para>
    ///     <para>
    ///         Commands arrive fully formed, with their own subcommands and parameters already declared.
    ///         There is nothing to compose here on purpose: a command is a type, so the hierarchy is read
    ///         from the constructors of those types rather than assembled twice.
    ///     </para>
    /// </remarks>
    public interface ICommandRegistryBuilder
    {
        /// <summary>
        ///     Registers a command reachable by naming it first.
        /// </summary>
        /// <remarks>
        ///     The whole subtree below it is checked over as well, so a command that could never be reached
        ///     or never be run is heard about here and not from a console line that fails to do anything.
        /// </remarks>
        /// <param name="command">Command to register.</param>
        /// <returns>Returns the builder for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a command of the same name is already registered, or when the command or one below
        ///     it cannot be run, holds subcommands next to positional arguments, declares a required argument
        ///     behind an optional one, or holds two subcommands of the same name.
        /// </exception>
        ICommandRegistryBuilder Add(ICommand command);

        /// <summary>
        ///     Replaces the way the lines of input are read.
        /// </summary>
        /// <remarks>
        ///     The place to hand in a parser of another syntax, or the default one carrying a converter that
        ///     knows the types of the game - <c>new CommandParser(new MyValueConverter())</c>.
        /// </remarks>
        /// <param name="parser">Parser reading the lines of input.</param>
        /// <returns>Returns the builder for chaining.</returns>
        ICommandRegistryBuilder WithParser(ICommandParser parser);

        /// <summary>
        ///     Adds the built-in help to the commands the manager is built with.
        /// </summary>
        /// <remarks>
        ///     The help is created by <see cref="Build"/>, when every command it is to describe is known, and it
        ///     describes those - including the ones registered after this call. It answers to
        ///     <see cref="Help.HelpCommand.CommandName"/>, so no registered command may bear that name.
        /// </remarks>
        /// <returns>Returns the builder for chaining.</returns>
        ICommandRegistryBuilder WithHelp();

        /// <summary>
        ///     Creates the manager from the current registrations.
        /// </summary>
        /// <remarks>
        ///     Registrations are copied, so further changes of the builder do not affect already created
        ///     managers.
        /// </remarks>
        /// <returns>Returns the manager serving the registered commands.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the help was asked for but a registered command already bears its name.</exception>
        ICommandManager Build();
    }
}

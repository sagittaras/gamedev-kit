using System.Collections.Generic;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parameters;

namespace Sagittaras.Commands
{
    /// <summary>
    ///     Describes a single, available command.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The interface is description only - everything the parser needs to recognize the command on
    ///         the input, and nothing about running it. Invoking belongs to <see cref="ISyncCommand"/> and
    ///         <see cref="IAsyncCommand"/>, so it is one of those two a command implements; this one is the
    ///         common base they are both reached through.
    ///     </para>
    ///     <para>
    ///         Keeping the two apart is what lets the parser resolve an input without ever being able to
    ///         run what it resolved, and lets a command be invoked without a parser having been involved at
    ///         all - a button in the editor tooling or a startup script composes an
    ///         <see cref="ICommandInput"/> of its own and calls the command directly.
    ///     </para>
    ///     <para>
    ///         A command either holds <see cref="Subcommands"/> or declares <see cref="Arguments"/>, never
    ///         both. <see cref="Options"/> are free of that rule, as they carry their own calling sign.
    ///     </para>
    /// </remarks>
    public interface ICommand
    {
        /// <summary>
        ///     Name of the command, serving as its calling sign.
        /// </summary>
        /// <remarks>
        ///     A single token, unique among the siblings it is resolved against - the commands of the
        ///     registry at the top level, the <see cref="Subcommands"/> of the parent below it. The full
        ///     path the input spells out is therefore never stated here, it is walked.
        /// </remarks>
        string Name { get; }

        /// <summary>
        ///     Short description of the command's usage.
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Commands reached by naming them behind this one.
        /// </summary>
        /// <remarks>
        ///     A command holding subcommands is a branch of the hierarchy, and reaching it without naming
        ///     one of them is not an error - the usual answer is the usage of what it offers.
        /// </remarks>
        IReadOnlyList<ICommand> Subcommands { get; }

        /// <summary>
        ///     Values the command reads from their place in the input, in the order it expects them.
        /// </summary>
        IReadOnlyList<IArgument> Arguments { get; }

        /// <summary>
        ///     Values the command reads by name, in any order.
        /// </summary>
        IReadOnlyList<IOption> Options { get; }
    }
}

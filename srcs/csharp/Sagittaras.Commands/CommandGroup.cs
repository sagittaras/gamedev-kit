using System;
using System.Collections.Generic;
using System.Linq;
using Sagittaras.Commands.Invocation;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Commands
{
    /// <summary>
    ///     A branch of the hierarchy, reached by naming one of the commands it holds.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Subcommands are added in the constructor, which makes the branch a type of its own with
    ///         somewhere to keep its description and its options:
    ///     </para>
    ///     <code>
    ///         public class SpawnCommand : CommandGroup
    ///         {
    ///             public SpawnCommand() : base("spawn", "Spawns things into the active scene")
    ///             {
    ///                 Add(new SpawnEntityCommand());
    ///                 Add(new SpawnItemCommand());
    ///             }
    ///         }
    ///     </code>
    ///     <para>
    ///         A group declares no positional arguments - that is the whole point of it being a group - but
    ///         options are fine, as they cannot be mistaken for the name of a subcommand. Being named
    ///         without one of them is not an error of the input as much as an unfinished thought, and
    ///         <see cref="Invoke"/> answers it with what the group offers.
    ///     </para>
    /// </remarks>
    public abstract class CommandGroup : CommandBase, ISyncCommand
    {
        /// <summary>
        ///     Commands reached by naming them behind this one.
        /// </summary>
        private readonly List<ICommand> _subcommands = new();

        /// <summary>
        ///     Creates a group answering to the given name.
        /// </summary>
        /// <param name="name">Name of the command, serving as its calling sign.</param>
        /// <param name="description">Short description of what the group gathers.</param>
        protected CommandGroup(string name, string description)
            : base(name, description)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyList<ICommand> Subcommands => _subcommands;

        /// <summary>
        ///     Answers the group being named without one of its subcommands.
        /// </summary>
        /// <remarks>
        ///     Reports what the group offers, as there is nothing else it could have been asked for. A group
        ///     with something better to say - a state of its own, a default subcommand - overrides this.
        /// </remarks>
        /// <param name="input">Values bound for this invocation.</param>
        /// <returns>Returns the <see cref="UsageOutput"/> of the group.</returns>
        public virtual ICommandOutput Invoke(ICommandInput input)
        {
            return new UsageOutput(this);
        }

        /// <summary>
        ///     Adds a command reached by naming it behind this one.
        /// </summary>
        /// <remarks>
        ///     Names are told apart the way the input is matched against them, which ignores the case - so
        ///     two siblings differing only by it would leave one of them unreachable.
        /// </remarks>
        /// <param name="subcommand">Command to add.</param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the group already declares a positional argument, or already holds a command of
        ///     the same name.
        /// </exception>
        protected void Add(ICommand subcommand)
        {
            Guard.Against.True(Arguments.Count > 0, $"Command '{Name}' declares positional arguments, so it cannot hold the subcommand '{subcommand.Name}' - a token behind it would be both a subcommand name and the value of an argument.");
            Guard.Against.True(HoldsName(subcommand.Name), $"Command '{Name}' already holds a subcommand named '{subcommand.Name}', and only one of the two could ever be reached.");

            _subcommands.Add(subcommand);
        }

        /// <summary>
        ///     Tells whether the group already holds a command of the given name.
        /// </summary>
        /// <param name="name">Name to look for.</param>
        /// <returns>Returns true when a subcommand of that name is already held; otherwise false.</returns>
        private bool HoldsName(string name)
        {
            return _subcommands.Any(subcommand => string.Equals(subcommand.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}

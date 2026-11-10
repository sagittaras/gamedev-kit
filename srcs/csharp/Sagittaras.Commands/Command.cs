using Sagittaras.Commands.Invocation;

namespace Sagittaras.Commands
{
    /// <summary>
    ///     A command doing its work within the call that invoked it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The shape most commands are written in. Parameters are declared in the constructor and read
    ///         back in <see cref="Invoke"/> through the very same handles:
    ///     </para>
    ///     <code>
    ///         public class SpawnEntityCommand : Command
    ///         {
    ///             private static readonly Argument&lt;string&gt; Entity = new("entity", "Entity to spawn");
    ///             private static readonly Option&lt;int&gt; Count = new("count", "How many to spawn", 1, "c");
    ///
    ///             public SpawnEntityCommand() : base("entity", "Spawns an entity into the active scene")
    ///             {
    ///                 Accepts(Entity, Count);
    ///             }
    ///
    ///             public override ICommandOutput Invoke(ICommandInput input)
    ///             {
    ///                 return CommandOutput.Success($"Spawned {input.GetValue&lt;int&gt;(Count)}x {input.GetValue&lt;string&gt;(Entity)}");
    ///             }
    ///         }
    ///     </code>
    ///     <para>
    ///         A command written this way cannot hold subcommands - branches of the hierarchy are
    ///         <see cref="CommandGroup"/>, which is what keeps a token behind the command from meaning two
    ///         things at once. Work that has to be awaited belongs to <see cref="AsyncCommand"/> instead.
    ///     </para>
    /// </remarks>
    public abstract class Command : CommandBase, ISyncCommand
    {
        /// <summary>
        ///     Creates a command answering to the given name.
        /// </summary>
        /// <param name="name">Name of the command, serving as its calling sign.</param>
        /// <param name="description">Short description of the command's usage.</param>
        protected Command(string name, string description)
            : base(name, description)
        {
        }

        /// <inheritdoc />
        public abstract ICommandOutput Invoke(ICommandInput input);
    }
}

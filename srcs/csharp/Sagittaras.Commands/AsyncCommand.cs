using System.Threading;
using System.Threading.Tasks;
using Sagittaras.Commands.Invocation;

namespace Sagittaras.Commands
{
    /// <summary>
    ///     A command whose work has to be awaited.
    /// </summary>
    /// <remarks>
    ///     Written exactly like a <see cref="Command"/> - parameters declared in the constructor, values
    ///     read back through the same handles - and reached only through the awaiting entrypoint of the
    ///     manager. Reach for it when the work genuinely leaves the calling frame, such as loading a scene
    ///     or asking the game server; anything answering right away is better off synchronous.
    /// </remarks>
    public abstract class AsyncCommand : CommandBase, IAsyncCommand
    {
        /// <summary>
        ///     Creates a command answering to the given name.
        /// </summary>
        /// <param name="name">Name of the command, serving as its calling sign.</param>
        /// <param name="description">Short description of the command's usage.</param>
        protected AsyncCommand(string name, string description)
            : base(name, description)
        {
        }

        /// <inheritdoc />
        public abstract ValueTask<ICommandOutput> InvokeAsync(ICommandInput input, CancellationToken cancellationToken = default);
    }
}

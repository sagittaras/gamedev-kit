using System.Threading;
using System.Threading.Tasks;
using Sagittaras.Commands.Invocation;

namespace Sagittaras.Commands
{
    /// <summary>
    ///     A command whose answer has to be awaited.
    /// </summary>
    /// <remarks>
    ///     For work that genuinely leaves the calling frame - loading a scene, asking the game server,
    ///     waiting for the next frame. A command is either this or <see cref="ISyncCommand"/>, never both,
    ///     and being asynchronous costs it the synchronous entrypoint of the manager: blocking on the
    ///     result would deadlock on the very thread a developer console tends to live on.
    /// </remarks>
    public interface IAsyncCommand : ICommand
    {
        /// <summary>
        ///     Runs the command over the values it was given.
        /// </summary>
        /// <remarks>
        ///     Bound the same way and under the same guarantees as <see cref="ISyncCommand.Invoke"/> -
        ///     what reaches this point is an input the parser has already made sense of.
        /// </remarks>
        /// <param name="input">Values bound for this invocation.</param>
        /// <param name="cancellationToken">Token cancelling the invocation, raised when the console abandons it.</param>
        /// <returns>Returns the result of the invocation.</returns>
        ValueTask<ICommandOutput> InvokeAsync(ICommandInput input, CancellationToken cancellationToken = default);
    }
}

using Sagittaras.Commands.Invocation;

namespace Sagittaras.Commands
{
    /// <summary>
    ///     A command answering within the call that invoked it.
    /// </summary>
    /// <remarks>
    ///     The common case, and the one to reach for unless the work genuinely has to be awaited -
    ///     toggling a debug flag, printing a state, spawning an entity. The asynchronous counterpart is
    ///     <see cref="IAsyncCommand"/>, and a command is one or the other, never both.
    /// </remarks>
    public interface ISyncCommand : ICommand
    {
        /// <summary>
        ///     Runs the command over the values it was given.
        /// </summary>
        /// <remarks>
        ///     Everything the input could be wrong about has been decided before this is reached: the
        ///     command was resolved, the values converted, the defaults filled in. What is left to fail
        ///     here belongs to the command itself - a missing entity, a scene that is not loaded - and is
        ///     answered by the returned output, not by an exception.
        /// </remarks>
        /// <param name="input">Values bound for this invocation.</param>
        /// <returns>Returns the result of the invocation.</returns>
        ICommandOutput Invoke(ICommandInput input);
    }
}

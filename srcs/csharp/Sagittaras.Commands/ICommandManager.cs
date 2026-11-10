using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parsing;

namespace Sagittaras.Commands
{
    /// <summary>
    ///     The entrypoint a console reaches the registered commands through.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Built once by <see cref="Building.ICommandRegistryBuilder"/> and only read afterwards, so a
    ///         line typed into the console costs a walk of the hierarchy and nothing else.
    ///     </para>
    ///     <para>
    ///         What used to be the root of the hierarchy is the manager itself: it holds the commands the
    ///         first token is resolved against, and the parsing is a service it asks. A console that only
    ///         wants to run a line reaches for <c>Execute</c>; one that wants to show what a line means
    ///         before committing to it parses first and runs the result it already has.
    ///     </para>
    /// </remarks>
    public interface ICommandManager
    {
        /// <summary>
        ///     Commands the first token of an input is resolved against.
        /// </summary>
        IReadOnlyList<ICommand> Commands { get; }

        /// <summary>
        ///     Reads the input without running anything.
        /// </summary>
        /// <remarks>
        ///     Worth reaching for to answer a line before it is committed to - completing it, highlighting
        ///     it, telling what it would do - and to tell a synchronous command from an asynchronous one
        ///     before choosing how to run it.
        /// </remarks>
        /// <param name="input">Line of input, as it was typed.</param>
        /// <returns>Returns what the parser made of the input, successful or not.</returns>
        IParseResult Parse(string input);

        /// <summary>
        ///     Reads the input and runs what it resolved to.
        /// </summary>
        /// <remarks>
        ///     An input the parser could not make sense of is answered with a <see cref="RefusalOutput"/>
        ///     carrying the failed parsing, not with an exception - a console has somewhere to print either, can
        ///     point at the word that was wrong, and a typo is not worth a stack trace.
        /// </remarks>
        /// <param name="input">Line of input, as it was typed.</param>
        /// <returns>Returns the result of the invocation, or the reason the input could not be read.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the input resolves to an <see cref="IAsyncCommand"/>.</exception>
        ICommandOutput Execute(string input);

        /// <summary>
        ///     Runs what an already parsed input resolved to.
        /// </summary>
        /// <param name="result">Result of parsing an input.</param>
        /// <returns>Returns the result of the invocation, or the reason the input could not be read.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the result resolves to an <see cref="IAsyncCommand"/>.</exception>
        ICommandOutput Execute(IParseResult result);

        /// <summary>
        ///     Reads the input and awaits what it resolved to.
        /// </summary>
        /// <remarks>
        ///     Reaches both kinds of command - a synchronous one answers within the call and the task is
        ///     already completed - so a console with an awaiting loop needs nothing else. The synchronous
        ///     entrypoint deliberately refuses an <see cref="IAsyncCommand"/> instead of blocking on it,
        ///     which on the thread a console tends to live on would deadlock rather than wait.
        /// </remarks>
        /// <param name="input">Line of input, as it was typed.</param>
        /// <param name="cancellationToken">Token cancelling the invocation, raised when the console abandons it.</param>
        /// <returns>Returns the result of the invocation, or the reason the input could not be read.</returns>
        ValueTask<ICommandOutput> ExecuteAsync(string input, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Awaits what an already parsed input resolved to.
        /// </summary>
        /// <param name="result">Result of parsing an input.</param>
        /// <param name="cancellationToken">Token cancelling the invocation, raised when the console abandons it.</param>
        /// <returns>Returns the result of the invocation, or the reason the input could not be read.</returns>
        ValueTask<ICommandOutput> ExecuteAsync(IParseResult result, CancellationToken cancellationToken = default);
    }
}

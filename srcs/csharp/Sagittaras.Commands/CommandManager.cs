using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parsing;

namespace Sagittaras.Commands
{
    /// <summary>
    ///     Holds the registered commands and reaches them through the parser it was built with.
    /// </summary>
    public class CommandManager : ICommandManager
    {
        /// <summary>
        ///     Parser reading the lines of input against the registered commands.
        /// </summary>
        private readonly ICommandParser _parser;

        /// <summary>
        ///     Creates a manager over the registered commands.
        /// </summary>
        /// <param name="commands">Commands the first token of an input is resolved against.</param>
        /// <param name="parser">Parser reading the lines of input.</param>
        internal CommandManager(IReadOnlyList<ICommand> commands, ICommandParser parser)
        {
            Commands = commands;
            _parser = parser;
        }

        /// <inheritdoc />
        public IReadOnlyList<ICommand> Commands { get; }

        /// <inheritdoc />
        public IParseResult Parse(string input)
        {
            return _parser.Parse(input, Commands);
        }

        /// <inheritdoc />
        public ICommandOutput Execute(string input)
        {
            return Execute(Parse(input));
        }

        /// <inheritdoc />
        public ICommandOutput Execute(IParseResult result)
        {
            if (!result.Success)
            {
                return Refuse(result);
            }

            return result.Command is ISyncCommand sync
                ? sync.Invoke(result.Input!)
                : throw new InvalidOperationException($"Command '{result.Command!.Name}' has to be awaited, so it cannot be run through the synchronous entrypoint - reach for ExecuteAsync instead.");
        }

        /// <inheritdoc />
        public ValueTask<ICommandOutput> ExecuteAsync(string input, CancellationToken cancellationToken = default)
        {
            return ExecuteAsync(Parse(input), cancellationToken);
        }

        /// <inheritdoc />
        public ValueTask<ICommandOutput> ExecuteAsync(IParseResult result, CancellationToken cancellationToken = default)
        {
            if (!result.Success)
            {
                return new ValueTask<ICommandOutput>(Refuse(result));
            }

            return result.Command switch
            {
                IAsyncCommand asynchronous => asynchronous.InvokeAsync(result.Input!, cancellationToken),
                ISyncCommand synchronous => new ValueTask<ICommandOutput>(synchronous.Invoke(result.Input!)),
                _ => throw new InvalidOperationException($"Command '{result.Command!.Name}' is neither synchronous nor asynchronous, so there is no way to run it.")
            };
        }

        /// <summary>
        ///     Answers an input the parser could not make sense of.
        /// </summary>
        /// <param name="result">Result of parsing an input.</param>
        /// <returns>Returns the <see cref="RefusalOutput"/> carrying the failed parsing.</returns>
        private static ICommandOutput Refuse(IParseResult result)
        {
            return new RefusalOutput(result);
        }
    }
}

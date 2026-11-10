using System.Collections.Generic;

namespace Sagittaras.Commands.Parsing
{
    /// <summary>
    ///     Reads a line of input as one of the commands it is offered.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The parser is handed the commands to resolve against and gives back what it made of the
    ///         input. It never runs anything - <see cref="IParseResult"/> is the whole of its answer - which
    ///         is what lets a console show what an input means before it is committed to, and what keeps a
    ///         command reachable without a parser at all.
    ///     </para>
    ///     <para>
    ///         It holds no state between calls, so the one built by the registry is shared for the lifetime
    ///         of the manager and answers every line typed into the console.
    ///     </para>
    /// </remarks>
    public interface ICommandParser
    {
        /// <summary>
        ///     Reads the input as one of the given commands.
        /// </summary>
        /// <param name="input">Line of input, as it was typed.</param>
        /// <param name="commands">Commands the first token is resolved against, the roots of the hierarchy.</param>
        /// <returns>Returns what the parser made of the input, successful or not.</returns>
        IParseResult Parse(string input, IReadOnlyList<ICommand> commands);
    }
}

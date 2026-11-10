namespace Sagittaras.Commands.Invocation
{
    /// <summary>
    ///     Represents the result of the command invocation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Deliberately an interface and not a closed type. A command whose answer is more than a line
    ///         of text - a table of items, a tree of entities, a diff of two states - implements its own
    ///         output carrying that payload, and the layer rendering it picks the formatter by the type it
    ///         was handed. <see cref="Content"/> stays the plain reading of the same answer, for everything
    ///         that has no formatter of its own to reach for.
    ///     </para>
    ///     <para>
    ///         That is also why the invocation reports failure by returning an output rather than by
    ///         throwing: a command that could not do its work still has something to say about it, and the
    ///         console has somewhere to put it.
    ///     </para>
    /// </remarks>
    public interface ICommandOutput
    {
        /// <summary>
        ///     Exit code of the command invocation.
        /// </summary>
        /// <remarks>
        ///     Zero indicates success, non-zero indicates failure.
        /// </remarks>
        int ExitCode { get; }

        /// <summary>
        ///     Content of the command invocation.
        /// </summary>
        string Content { get; }
    }
}

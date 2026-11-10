namespace Sagittaras.Commands.Parameters
{
    /// <summary>
    ///     An option whose whole value is whether the input named it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Reads as false until the input names it, and takes no value of its own:
    ///     </para>
    ///     <code>
    ///         private static readonly Flag DryRun = new("dry-run", "Report the work without doing it");
    ///     </code>
    ///     <para>
    ///         A type of its own rather than an <see cref="Option{T}"/> of bool that guesses, because
    ///         <c>--verbose true</c> is a perfectly ordinary option and only the declaration can tell the
    ///         two apart.
    ///     </para>
    /// </remarks>
    public sealed class Flag : Option<bool>
    {
        /// <summary>
        ///     Declares a flag the input may name.
        /// </summary>
        /// <param name="name">Name the flag is called by, spelled without the prefix the input uses.</param>
        /// <param name="description">Description of what naming the flag means to the command.</param>
        /// <param name="aliases">Alternative names the flag answers to, spelled without the prefix.</param>
        public Flag(string name, string description, params string[] aliases)
            : base(name, description, false, true, aliases)
        {
        }
    }
}

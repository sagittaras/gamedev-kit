using Sagittaras.Commands.Invocation;

namespace Sagittaras.Commands.Parameters
{
    /// <summary>
    ///     A value the command reads from its place in the input, declared with the type it is read as.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Meant to be held as a field of the command and read back through the very same instance, so
    ///         that <see cref="ICommandInput"/> needs no names and a typo cannot survive the compilation:
    ///     </para>
    ///     <code>
    ///         private static readonly Argument&lt;string&gt; Entity = new("entity", "Entity to spawn");
    ///     </code>
    ///     <para>
    ///         An argument is required unless it is given a default, and giving it one is the only way to
    ///         make it optional. The two cannot be told apart in the input anyway - an argument that may be
    ///         missing has to answer with something once it is - and keeping them one decision means every
    ///         declared parameter always has a value to read.
    ///     </para>
    ///     <para>
    ///         A rule of this one argument, rather than of its type, is handed over as its own parser:
    ///     </para>
    ///     <code>
    ///         private static readonly Argument&lt;int&gt; Percent = new("percent", "Share of the health to restore")
    ///         {
    ///             Parser = text => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) &amp;&amp; value is &gt;= 0 and &lt;= 100
    ///                 ? ParsedValue&lt;int&gt;.Read(value)
    ///                 : ParsedValue&lt;int&gt;.Refused($"The share has to be a whole number between 0 and 100, and '{text}' is not.")
    ///         };
    ///     </code>
    ///     <para>
    ///         A rule over a value the converter reads well on its own is better held by a validator, which
    ///         sees the value already read and only judges it:
    ///     </para>
    ///     <code>
    ///         private static readonly Argument&lt;int&gt; Count = new("count", "How many to summon", 1)
    ///         {
    ///             Validator = count => count is &gt;= 1 and &lt;= 10 ? null : $"Between 1 and 10 can be summoned at once, not {count}."
    ///         };
    ///     </code>
    /// </remarks>
    /// <typeparam name="T">Type the value is read as.</typeparam>
    public class Argument<T> : Parameter<T>, IArgument
    {
        /// <summary>
        ///     Declares an argument the input has to carry.
        /// </summary>
        /// <param name="name">Name of the argument, as the help and the parsing errors spell it.</param>
        /// <param name="description">Description of what the value means to the command.</param>
        /// <exception cref="System.ArgumentException">Thrown when the name is empty or holds white space.</exception>
        public Argument(string name, string description)
            : base("Argument", name, description, false, null)
        {
        }

        /// <summary>
        ///     Declares an argument the input may leave out, falling back to the given value.
        /// </summary>
        /// <param name="name">Name of the argument, as the help and the parsing errors spell it.</param>
        /// <param name="description">Description of what the value means to the command.</param>
        /// <param name="defaultValue">Value used when the input ends before reaching the argument.</param>
        /// <exception cref="System.ArgumentException">Thrown when the name is empty or holds white space.</exception>
        public Argument(string name, string description, T defaultValue)
            : base("Argument", name, description, true, defaultValue)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        ///     True exactly when the argument has a default value - there is no third state where the input
        ///     may leave it out with nothing to read in its place.
        /// </remarks>
        public bool IsOptional => HasDefaultValue;
    }
}

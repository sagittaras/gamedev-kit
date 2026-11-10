using System.Collections.Generic;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Commands.Parameters
{
    /// <summary>
    ///     A value the command reads by name, declared with the type it is read as.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Held as a field of the command the same way an <see cref="Argument{T}"/> is, and read back
    ///         through the same instance:
    ///     </para>
    ///     <code>
    ///         private static readonly Option&lt;int&gt; Count = new("count", "How many to spawn", 1, "c");
    ///     </code>
    ///     <para>
    ///         The default value is not optional here, because the option itself always is. Whatever the
    ///         input leaves unsaid, the command still reads a value - and an option that had to be there
    ///         would only be an argument with a name in front of it.
    ///     </para>
    ///     <para>
    ///         A rule of this one option, rather than of its type, is handed over the same way an argument takes
    ///         one - as <see cref="Parameter{T}.Parser"/> or <see cref="Parameter{T}.Validator"/>, set where the option is declared.
    ///     </para>
    /// </remarks>
    /// <typeparam name="T">Type the value is read as.</typeparam>
    public class Option<T> : Parameter<T>, IOption
    {
        /// <summary>
        ///     Declares an option the input may name, in any place and any order.
        /// </summary>
        /// <param name="name">Name the option is called by, spelled without the prefix the input uses.</param>
        /// <param name="description">Description of what the value means to the command.</param>
        /// <param name="defaultValue">Value used when the input does not name the option.</param>
        /// <param name="aliases">Alternative names the option answers to, spelled without the prefix.</param>
        /// <exception cref="System.ArgumentException">Thrown when the name or any of the aliases is empty or holds white space.</exception>
        public Option(string name, string description, T defaultValue, params string[] aliases)
            : this(name, description, defaultValue, false, aliases)
        {
        }

        /// <summary>
        ///     Declares an option, deciding whether naming it is the whole of its value.
        /// </summary>
        /// <remarks>
        ///     Reached through <see cref="Flag"/>, which is the only shape a valueless option makes sense in.
        ///     An option always has a default, as it is always allowed to be left out.
        /// </remarks>
        /// <param name="name">Name the option is called by, spelled without the prefix the input uses.</param>
        /// <param name="description">Description of what the value means to the command.</param>
        /// <param name="defaultValue">Value used when the input does not name the option.</param>
        /// <param name="isFlag">Whether the option takes no value of its own.</param>
        /// <param name="aliases">Alternative names the option answers to, spelled without the prefix.</param>
        /// <exception cref="System.ArgumentException">Thrown when the name or any of the aliases is empty or holds white space.</exception>
        protected Option(string name, string description, T defaultValue, bool isFlag, params string[] aliases)
            : base("Option", name, description, true, defaultValue)
        {
            GuardAliases(aliases);

            IsFlag = isFlag;
            Aliases = aliases;
        }

        /// <inheritdoc />
        public IReadOnlyList<string> Aliases { get; }

        /// <inheritdoc />
        public bool IsFlag { get; }

        /// <summary>
        ///     Ensures every alias is something the input could be matched against.
        /// </summary>
        /// <param name="aliases">Aliases to guard.</param>
        /// <exception cref="System.ArgumentException">Thrown when an alias is empty or holds white space.</exception>
        private static void GuardAliases(string[] aliases)
        {
            foreach (string alias in aliases)
            {
                Guard.Against.NullOrWhiteSpace(alias, "Option alias must not be empty.");
                Guard.Against.WhiteSpaceInside(alias, $"Option alias '{alias}' must be a single token - the input is matched token by token, so an alias holding white space could never be met as a whole.");
            }
        }
    }
}

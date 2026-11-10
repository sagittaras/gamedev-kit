using System.Collections.Generic;

namespace Sagittaras.Commands.Parameters
{
    /// <summary>
    ///     Describes a value the command reads by name, regardless of where it appears in the input.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An option carries its own calling sign, so it cannot be confused with anything else on the
    ///         input - which is why a command may declare options whether it holds subcommands or
    ///         arguments. Only positional <see cref="IArgument"/> collides with a subcommand name.
    ///     </para>
    ///     <para>
    ///         An option is always optional and always has a default: a value that must be named but also
    ///         must be present is better expressed as an argument. Nothing therefore fails to parse merely
    ///         because an option was left out.
    ///     </para>
    /// </remarks>
    public interface IOption : IParameter
    {
        /// <summary>
        ///     Alternative calling signs the option answers to, apart from its <see cref="IParameter.Name"/>.
        /// </summary>
        /// <remarks>
        ///     Aliases are written without the prefix the input spells them with - recognizing the prefix
        ///     belongs to the parser, not to the description of the option.
        /// </remarks>
        IReadOnlyList<string> Aliases { get; }

        /// <summary>
        ///     Indicates whether naming the option is the whole of its value.
        /// </summary>
        /// <remarks>
        ///     A flag takes no value from the input - being named sets it, being left out leaves it at its
        ///     default. Its <see cref="IParameter.ValueType"/> is therefore always <see cref="bool"/>.
        /// </remarks>
        bool IsFlag { get; }
    }
}

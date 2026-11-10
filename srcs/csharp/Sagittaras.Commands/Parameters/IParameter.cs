using System;
using Sagittaras.Commands.Invocation;

namespace Sagittaras.Commands.Parameters
{
    /// <summary>
    ///     Describes a single value a command accepts on its input.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Shared by both ways a value can reach the command - positionally as an
    ///         <see cref="IArgument"/>, or by name as an <see cref="IOption"/>. The distinction matters
    ///         only while the input is being read; once the value is bound, nothing about it depends on
    ///         the form it arrived in, which is why <see cref="ICommandInput"/> looks values up by the
    ///         parameter alone.
    ///     </para>
    ///     <para>
    ///         Parameters are description, not storage. The very same instance serves every invocation of
    ///         the command, so it never holds a value - it is the key under which the bound one is found.
    ///     </para>
    /// </remarks>
    public interface IParameter
    {
        /// <summary>
        ///     Gets the .NET type the input value is converted to.
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        ///     Name of the parameter.
        /// </summary>
        /// <remarks>
        ///     Serves the generated help and the parsing errors. For an <see cref="IOption"/> it is also
        ///     the calling sign the input is matched against.
        /// </remarks>
        string Name { get; }

        /// <summary>
        ///     Description of what the value means to the command.
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Indicates whether the parameter falls back to a value of its own when the input omits it.
        /// </summary>
        bool HasDefaultValue { get; }

        /// <summary>
        ///     The value used when the input omits the parameter.
        /// </summary>
        /// <remarks>
        ///     Meaningful only while <see cref="HasDefaultValue"/> is true. Null is a legitimate default of
        ///     its own, so the flag is what decides, not the value being null.
        /// </remarks>
        object? DefaultValue { get; }

        /// <summary>
        ///     Reads the value from a piece of the input in place of the converter, or null when the converter
        ///     reads it.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         For the rules of a single parameter rather than of a type - a share between nought and a
        ///         hundred, an entity that is actually in the scene. None of those is a property of an integer
        ///         or a string, so none of them belongs to the converter, and a refusal can say which rule the
        ///         text broke instead of only naming the type it was not.
        ///     </para>
        ///     <para>
        ///         The parameter only carries the parser; it is the binding that calls it, and the parameter
        ///         stays a description of what it accepts. Null is the usual case - the converter reads the
        ///         value as <see cref="ValueType"/>.
        ///     </para>
        /// </remarks>
        Func<string, ParsedValue<object?>>? Parser { get; }

        /// <summary>
        ///     Judges a value that has already been read, or null when every value of the type is accepted.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Answers null for a value it accepts, and the reason it does not otherwise - in words meant for
        ///         whoever typed the input, as they reach the console unchanged.
        ///     </para>
        ///     <para>
        ///         It comes after the reading, whether the converter or <see cref="Parser"/> did it, and sees the
        ///         value rather than the text. That makes it the place for a rule over a value the converter reads
        ///         well on its own - a number within a range - without the parameter having to read the number
        ///         itself and risk reading it differently than the rest of the input is read.
        ///     </para>
        /// </remarks>
        Func<object?, string?>? Validator { get; }
    }
}

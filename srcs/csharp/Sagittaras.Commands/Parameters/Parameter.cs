using System;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Commands.Parameters
{
    /// <summary>
    ///     What an argument and an option have in common - a name, a description, a default, and the way
    ///     their values are read and judged.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The two differ only in how the input reaches them: an argument by its place, an option by its
    ///         name. Everything about the value itself is the same, so it is said here once, and
    ///         <see cref="Argument{T}"/> and <see cref="Option{T}"/> add nothing but what their kind needs.
    ///     </para>
    ///     <para>
    ///         It cannot be derived from outside the package. Argument and option are the only two kinds the
    ///         parser knows how to read, so a third one could be declared but never reached - which is
    ///         better refused by the compiler than by the command it is declared on.
    ///     </para>
    /// </remarks>
    /// <typeparam name="T">Type the value is read as.</typeparam>
    public abstract class Parameter<T> : IParameter
    {
        /// <summary>
        ///     Own parser of the parameter, or null while the converter reads the value.
        /// </summary>
        private readonly Func<string, ParsedValue<T>>? _parser;

        /// <summary>
        ///     The same parser handing the value over as an object, built once for the binding to call.
        /// </summary>
        private readonly Func<string, ParsedValue<object?>>? _untypedParser;

        /// <summary>
        ///     Validator of the parameter, or null while every value of the type is accepted.
        /// </summary>
        private readonly Func<T, string?>? _validator;

        /// <summary>
        ///     The same validator taking the value as an object, built once for the binding to call.
        /// </summary>
        private readonly Func<object?, string?>? _untypedValidator;

        /// <summary>
        ///     Declares a parameter of the given kind.
        /// </summary>
        /// <param name="kind">How the kind of the parameter is spoken of in the exceptions.</param>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="description">Description of what the value means to the command.</param>
        /// <param name="hasDefaultValue">Whether the parameter falls back to a value of its own.</param>
        /// <param name="defaultValue">Value used when the input leaves the parameter out.</param>
        /// <exception cref="ArgumentException">Thrown when the name is empty or holds white space.</exception>
        private protected Parameter(string kind, string name, string description, bool hasDefaultValue, object? defaultValue)
        {
            Guard.Against.NullOrWhiteSpace(name, $"{kind} must be given a name.");
            Guard.Against.WhiteSpaceInside(name, $"{kind} name '{name}' must be a single token, as it is spelled as one word wherever it appears.");

            Name = name;
            Description = description;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;
        }

        /// <inheritdoc />
        public Type ValueType => typeof(T);

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public bool HasDefaultValue { get; }

        /// <inheritdoc />
        public object? DefaultValue { get; }

        /// <summary>
        ///     Reads the value from a piece of the input in place of the converter.
        /// </summary>
        /// <remarks>
        ///     Set once, while the parameter is declared. Null leaves the reading to the converter, as
        ///     <see cref="IParameter.Parser"/> describes.
        /// </remarks>
        public Func<string, ParsedValue<T>>? Parser
        {
            get => _parser;
            init
            {
                _parser = value;
                _untypedParser = value == null ? null : text => value(text).ToUntyped();
            }
        }

        /// <inheritdoc />
        Func<string, ParsedValue<object?>>? IParameter.Parser => _untypedParser;

        /// <summary>
        ///     Judges a value that has already been read, answering null for one it accepts.
        /// </summary>
        /// <remarks>
        ///     Set once, while the parameter is declared, and run after the reading as
        ///     <see cref="IParameter.Validator"/> describes. Several rules are one validator checking them in
        ///     turn.
        /// </remarks>
        public Func<T, string?>? Validator
        {
            get => _validator;
            init
            {
                _validator = value;
                _untypedValidator = value == null ? null : bound => value((T)bound!);
            }
        }

        /// <inheritdoc />
        Func<object?, string?>? IParameter.Validator => _untypedValidator;
    }
}

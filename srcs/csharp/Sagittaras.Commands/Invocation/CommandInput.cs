using System;
using System.Collections.Generic;
using Sagittaras.Commands.Parameters;

namespace Sagittaras.Commands.Invocation
{
    /// <summary>
    ///     The values a single invocation of a command was given.
    /// </summary>
    /// <remarks>
    ///     Built by the parser out of a line of input, or composed directly by whoever has the values
    ///     already - a button in the editor tooling, a startup script, a test. Either way the command sees
    ///     the same thing, which is what keeps the parsing out of the invocation.
    /// </remarks>
    public class CommandInput : ICommandInput
    {
        /// <summary>
        ///     Values bound for the parameters of the command, each knowing whether the input named it.
        /// </summary>
        private readonly Dictionary<IParameter, BoundValue> _values;

        /// <summary>
        ///     Composes an input for the command out of the values that are already known.
        /// </summary>
        /// <remarks>
        ///     Parameters left out fall back to their own default, and are reported as not specified. The given
        ///     values are held to the same rules a line of input is: they have to be of the declared type and
        ///     pass the validator of their parameter. Defaults are not judged, as nobody gave them.
        /// </remarks>
        /// <param name="command">Command the values are meant for.</param>
        /// <param name="values">Values known for the parameters of the command.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when a value is given for a parameter the command does not declare, when a required
        ///     argument is left without one, when a given value is not of the declared type, or when the
        ///     validator of its parameter refuses it.
        /// </exception>
        public CommandInput(ICommand command, IReadOnlyDictionary<IParameter, object?> values)
        {
            _values = new();

            foreach (IArgument argument in command.Arguments)
            {
                Take(argument, values, argument.IsOptional);
            }

            foreach (IOption option in command.Options)
            {
                Take(option, values, true);
            }

            RequireDeclared(command, values);
        }

        /// <summary>
        ///     Creates an input out of what the binding arrived at.
        /// </summary>
        /// <param name="values">Values bound for the parameters of the command.</param>
        internal CommandInput(Dictionary<IParameter, BoundValue> values)
        {
            _values = values;
        }

        /// <inheritdoc />
        public bool IsSpecified(IParameter parameter)
        {
            return _values.TryGetValue(parameter, out BoundValue bound) && bound.IsSpecified;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">Thrown when the parameter is not one this input was built for.</exception>
        public T GetValue<T>(IParameter parameter)
        {
            return _values.TryGetValue(parameter, out BoundValue bound)
                ? (T)bound.Value!
                : throw new ArgumentException($"Parameter '{parameter.Name}' is not one this input was built for, so there is no value to read.", nameof(parameter));
        }

        /// <inheritdoc />
        public bool TryGetValue<T>(IParameter parameter, out T value)
        {
            if (_values.TryGetValue(parameter, out BoundValue bound) && bound.Value is T typed)
            {
                value = typed;
                return true;
            }

            value = default!;
            return false;
        }

        /// <summary>
        ///     Takes the value known for the parameter, or falls back to its default.
        /// </summary>
        /// <param name="parameter">Parameter to take the value of.</param>
        /// <param name="values">Values known for the parameters of the command.</param>
        /// <param name="mayBeMissing">Whether the parameter is allowed to be left out.</param>
        /// <exception cref="ArgumentException">Thrown when a parameter that has to be given is left out, or when a given value is refused.</exception>
        private void Take(IParameter parameter, IReadOnlyDictionary<IParameter, object?> values, bool mayBeMissing)
        {
            if (values.TryGetValue(parameter, out object? value))
            {
                RequireAccepted(parameter, value, nameof(values));
                _values[parameter] = BoundValue.Given(value);

                return;
            }

            if (!mayBeMissing)
            {
                throw new ArgumentException($"Argument '{parameter.Name}' is required, so an input composed without it would not be one the command could run over.", nameof(values));
            }

            _values[parameter] = BoundValue.Defaulted(parameter.DefaultValue);
        }

        /// <summary>
        ///     Ensures a given value is one the parameter would have taken from a line of input.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         A composed input skips the parsing, but not the rules of the parameter - a command has to be
        ///         able to rely on its validator whether the value came from the console or from a button in the
        ///         editor tooling. The value is judged the same way the binding judges one, in the validator's own
        ///         words.
        ///     </para>
        ///     <para>
        ///         The type is checked first, as the validator judges a value of the declared type and a value of
        ///         another one would only break it with a cast nobody could read.
        ///     </para>
        /// </remarks>
        /// <param name="parameter">Parameter the value is given for.</param>
        /// <param name="value">Value given for the parameter.</param>
        /// <param name="paramName">Name of the argument the values arrived in, as reported by the exception.</param>
        /// <exception cref="ArgumentException">Thrown when the value is not of the declared type, or when the validator refuses it.</exception>
        private static void RequireAccepted(IParameter parameter, object? value, string paramName)
        {
            if (!Fits(parameter.ValueType, value))
            {
                throw new ArgumentException($"Parameter '{parameter.Name}' holds a value of type {parameter.ValueType.Name}, which '{value ?? "null"}' is not.", paramName);
            }

            string? refusal = parameter.Validator?.Invoke(value);

            if (refusal != null)
            {
                throw new ArgumentException(string.IsNullOrWhiteSpace(refusal) ? $"Parameter '{parameter.Name}' does not accept '{value}'." : refusal, paramName);
            }
        }

        /// <summary>
        ///     Tells whether the value can be held by a parameter of the given type.
        /// </summary>
        /// <param name="valueType">Type the parameter declares.</param>
        /// <param name="value">Value to ask about.</param>
        /// <returns>Returns true when the value is of the type, or null where the type allows it; otherwise false.</returns>
        private static bool Fits(Type valueType, object? value)
        {
            return value == null
                ? !valueType.IsValueType || Nullable.GetUnderlyingType(valueType) != null
                : valueType.IsInstanceOfType(value);
        }

        /// <summary>
        ///     Ensures no value was given for a parameter the command does not declare.
        /// </summary>
        /// <remarks>
        ///     A value nobody reads is a mistake worth hearing about - most often a parameter of another
        ///     command, or one that was renamed.
        /// </remarks>
        /// <param name="command">Command the values are meant for.</param>
        /// <param name="values">Values known for the parameters of the command.</param>
        /// <exception cref="ArgumentException">Thrown when a value is given for an undeclared parameter.</exception>
        private void RequireDeclared(ICommand command, IReadOnlyDictionary<IParameter, object?> values)
        {
            foreach (KeyValuePair<IParameter, object?> value in values)
            {
                if (!_values.ContainsKey(value.Key))
                {
                    throw new ArgumentException($"Command '{command.Name}' does not declare the parameter '{value.Key.Name}', so the value given for it would never be read.", nameof(values));
                }
            }
        }
    }
}

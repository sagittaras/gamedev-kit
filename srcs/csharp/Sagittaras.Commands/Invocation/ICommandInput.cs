using Sagittaras.Commands.Parameters;

namespace Sagittaras.Commands.Invocation
{
    /// <summary>
    ///     Carries the values a single invocation of a command was given.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is all a command ever sees of its input. It knows nothing of the text it may have come
    ///         from, nor of the tokens it was cut into - a command reading its values through this contract
    ///         works the same whether a console line, a button in the editor tooling or a startup script
    ///         put them there.
    ///     </para>
    ///     <para>
    ///         Values are looked up by the very <see cref="IParameter"/> instances the command declares, not
    ///         by name. A command therefore holds its parameters as fields and reads them back through the
    ///         same handles, and a typo cannot survive the compilation. Looking one up by name is still
    ///         possible through <see cref="ICommand.Arguments"/> and <see cref="ICommand.Options"/>, which
    ///         is where the names belong.
    ///     </para>
    /// </remarks>
    public interface ICommandInput
    {
        /// <summary>
        ///     Tells whether the input named the parameter itself.
        /// </summary>
        /// <remarks>
        ///     Separates a value that was given from one that was merely defaulted to, which the value
        ///     alone cannot tell - a flag left out and a flag named as false both read as false.
        /// </remarks>
        /// <param name="parameter">Parameter to ask about.</param>
        /// <returns>Returns true when the value came from the input; false when it comes from the default of the parameter.</returns>
        bool IsSpecified(IParameter parameter);

        /// <summary>
        ///     Gets the value bound for the parameter.
        /// </summary>
        /// <remarks>
        ///     Throws rather than answers with a default, because reaching for a parameter the command does
        ///     not declare is a mistake in the command, not in the input - whatever the input was missing
        ///     has been answered long before the invocation.
        /// </remarks>
        /// <param name="parameter">Parameter to read the value of.</param>
        /// <typeparam name="T">Type of the value, as declared by <see cref="IParameter.ValueType"/>.</typeparam>
        /// <returns>Returns the bound value, or the default of the parameter when the input omitted it.</returns>
        T GetValue<T>(IParameter parameter);

        /// <summary>
        ///     Tries to get the value bound for the parameter.
        /// </summary>
        /// <remarks>
        ///     Worth reaching for where a command shares its body with parameters it does not always
        ///     declare; for its own, <see cref="GetValue{T}"/> says more by throwing.
        /// </remarks>
        /// <param name="parameter">Parameter to read the value of.</param>
        /// <param name="value">Bound value, or the default of <typeparamref name="T"/> when there is none.</param>
        /// <typeparam name="T">Type of the value, as declared by <see cref="IParameter.ValueType"/>.</typeparam>
        /// <returns>Returns true when the input holds a value for the parameter; otherwise false.</returns>
        bool TryGetValue<T>(IParameter parameter, out T value);
    }
}

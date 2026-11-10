using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Commands.Parameters
{
    /// <summary>
    ///     What the own parser of a parameter made of a piece of the input.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Either the value that was read, or the reason it was not - told in words meant for whoever typed
    ///         the input, as they reach the console unchanged. That is the whole difference from the converter,
    ///         which can only say that a text is not the declared type: a parser knows the rule it holds the
    ///         value to, and says which one was broken.
    ///     </para>
    ///     <para>
    ///         The default of the struct is a refusal with no reason given, and is answered with the same
    ///         reading a mismatched type would get.
    ///     </para>
    /// </remarks>
    /// <typeparam name="T">Type the value is read as.</typeparam>
    public readonly struct ParsedValue<T>
    {
        /// <summary>
        ///     Creates a parsed value.
        /// </summary>
        /// <param name="isRead">Whether the text was read.</param>
        /// <param name="value">Value that was read.</param>
        /// <param name="message">Reason the text was refused.</param>
        private ParsedValue(bool isRead, T value, string? message)
        {
            IsRead = isRead;
            Value = value;
            Message = message;
        }

        /// <summary>
        ///     Whether the text was read as a value.
        /// </summary>
        public bool IsRead { get; }

        /// <summary>
        ///     Value that was read.
        /// </summary>
        /// <remarks>
        ///     Meaningful only while <see cref="IsRead"/> is true.
        /// </remarks>
        public T Value { get; }

        /// <summary>
        ///     Reason the text was refused, meant for whoever typed the input.
        /// </summary>
        /// <remarks>
        ///     Null when the text was read, and for the default of the struct.
        /// </remarks>
        public string? Message { get; }

        /// <summary>
        ///     Reports a text that was read as a value.
        /// </summary>
        /// <param name="value">Value that was read.</param>
        /// <returns>Returns the parsed value.</returns>
        public static ParsedValue<T> Read(T value)
        {
            return new ParsedValue<T>(true, value, null);
        }

        /// <summary>
        ///     Reports a text that could not be read, and why.
        /// </summary>
        /// <param name="message">Reason the text was refused, meant for whoever typed the input.</param>
        /// <returns>Returns the refusal.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the reason is empty.</exception>
        public static ParsedValue<T> Refused(string message)
        {
            Guard.Against.NullOrWhiteSpace(message, "A refusal has to say why - the reason is what reaches the console.");

            return new ParsedValue<T>(false, default!, message);
        }

        /// <summary>
        ///     Hands the result over with the value as an object, the way the binding works with it.
        /// </summary>
        /// <returns>Returns the same result, typed as object.</returns>
        internal ParsedValue<object?> ToUntyped()
        {
            return new ParsedValue<object?>(IsRead, Value, Message);
        }
    }
}

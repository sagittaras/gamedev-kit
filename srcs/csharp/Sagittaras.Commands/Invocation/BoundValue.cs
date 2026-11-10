namespace Sagittaras.Commands.Invocation
{
    /// <summary>
    ///     A value bound for a parameter, together with whether the input named it.
    /// </summary>
    /// <remarks>
    ///     Keeping the two in one place is what makes a value that was bound but never marked - or marked but
    ///     never bound - impossible to arrive at. Held apart, in a dictionary of values and a set of the
    ///     specified parameters, they had to be kept in step by every line that touched either.
    /// </remarks>
    internal readonly struct BoundValue
    {
        /// <summary>
        ///     Creates a bound value.
        /// </summary>
        /// <param name="value">Value bound for the parameter.</param>
        /// <param name="isSpecified">Whether the value came from the input rather than from the default.</param>
        private BoundValue(object? value, bool isSpecified)
        {
            Value = value;
            IsSpecified = isSpecified;
        }

        /// <summary>
        ///     Value bound for the parameter.
        /// </summary>
        public object? Value { get; }

        /// <summary>
        ///     Whether the value came from the input rather than from the default of the parameter.
        /// </summary>
        public bool IsSpecified { get; }

        /// <summary>
        ///     Binds a value the input named itself.
        /// </summary>
        /// <param name="value">Value the input carried.</param>
        /// <returns>Returns the bound value, marked as specified.</returns>
        public static BoundValue Given(object? value)
        {
            return new BoundValue(value, true);
        }

        /// <summary>
        ///     Binds the value a parameter falls back to.
        /// </summary>
        /// <param name="value">Default of the parameter.</param>
        /// <returns>Returns the bound value, marked as not specified.</returns>
        public static BoundValue Defaulted(object? value)
        {
            return new BoundValue(value, false);
        }
    }
}

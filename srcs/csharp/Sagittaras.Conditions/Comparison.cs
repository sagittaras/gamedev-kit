namespace Sagittaras.Conditions
{
    /// <summary>
    ///     Comparison operator used for evaluation of the subject's value against the value expected by the condition.
    /// </summary>
    /// <remarks>
    ///     The subject's value always stands on the left side of the comparison, the expected value on the right side.
    /// </remarks>
    public enum Comparison
    {
        /// <summary>
        ///     Subject's value is equal to the expected value.
        /// </summary>
        Equals,

        /// <summary>
        ///     Subject's value is greater than the expected value.
        /// </summary>
        GreaterThan,

        /// <summary>
        ///     Subject's value is less than the expected value.
        /// </summary>
        LessThan,

        /// <summary>
        ///     Subject's value is greater than or equal to the expected value.
        /// </summary>
        GreaterOrEqual,

        /// <summary>
        ///     Subject's value is less than or equal to the expected value.
        /// </summary>
        LessOrEqual,
    }
}
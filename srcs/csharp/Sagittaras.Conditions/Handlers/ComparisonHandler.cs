using System;

namespace Sagittaras.Conditions.Handlers
{
    /// <summary>
    ///     Base handler for conditions comparing a value of the subject against the value expected by the condition
    ///     using the <see cref="ICondition.Comparison"/> operator.
    /// </summary>
    /// <remarks>
    ///     Derived handler only provides the actual value of the subject, for example its level. The expected value
    ///     is taken from the first parameter of the condition by default.
    /// </remarks>
    /// <typeparam name="TSubject">Subject against which the condition is evaluated.</typeparam>
    public abstract class ComparisonHandler<TSubject> : IConditionHandler<TSubject>
    {
        /// <inheritdoc />
        /// <exception cref="ArgumentException">Thrown when the condition does not provide the expected value.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the condition uses an unknown comparison operator.</exception>
        public bool Evaluate(ICondition condition, TSubject subject)
        {
            int actual = GetActualValue(condition, subject);
            int expected = GetExpectedValue(condition);

            return condition.Comparison switch
            {
                Comparison.Equals => actual == expected,
                Comparison.GreaterThan => actual > expected,
                Comparison.LessThan => actual < expected,
                Comparison.GreaterOrEqual => actual >= expected,
                Comparison.LessOrEqual => actual <= expected,
                _ => throw new ArgumentOutOfRangeException(nameof(condition), condition.Comparison, $"Condition type `{condition.ConditionType}` uses unknown comparison operator.")
            };
        }

        /// <summary>
        ///     Gets the actual value of the subject compared against the expected value.
        /// </summary>
        /// <param name="condition">Condition being evaluated.</param>
        /// <param name="subject">Subject against which the condition is evaluated.</param>
        /// <returns>Returns the value of the subject standing on the left side of the comparison.</returns>
        protected abstract int GetActualValue(ICondition condition, TSubject subject);

        /// <summary>
        ///     Gets the value expected by the condition.
        /// </summary>
        /// <remarks>
        ///     Override when the condition carries more parameters, for example an item identifier
        ///     followed by the required amount.
        /// </remarks>
        /// <param name="condition">Condition being evaluated.</param>
        /// <returns>Returns the value standing on the right side of the comparison.</returns>
        /// <exception cref="ArgumentException">Thrown when the condition has no parameters.</exception>
        protected virtual int GetExpectedValue(ICondition condition)
        {
            if (condition.Parameters.Count == 0)
            {
                throw new ArgumentException($"Condition type `{condition.ConditionType}` requires a parameter with the expected value.", nameof(condition));
            }

            return condition.Parameters[0];
        }
    }
}

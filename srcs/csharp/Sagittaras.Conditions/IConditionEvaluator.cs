namespace Sagittaras.Conditions
{
    /// <summary>
    ///     Represents an evaluation engine for the conditions.
    /// </summary>
    public interface IConditionEvaluator
    {
        /// <summary>
        ///     Evaluate whether the given condition is met.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="entity">Entity against which state the condition is evaluated.</param>
        /// <returns>Returns true if the condition is satisfied; otherwise false.</returns>
        bool Evaluate(ICondition condition, object entity);

        /// <summary>
        ///     Evaluates whether all conditions of the given conditional are met.
        /// </summary>
        /// <param name="conditional">The conditional to evaluate.</param>
        /// <param name="entity">Entity against which state the conditional is evaluated.</param>
        /// <returns>Returns true if all conditions are satisfied; otherwise false.</returns>
        bool Evaluate(IConditional conditional, object entity);
    }
}
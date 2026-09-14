namespace Sagittaras.Conditions.Evaluation
{
    /// <summary>
    ///     Represents an evaluation engine for the conditions.
    /// </summary>
    /// <typeparam name="TContext">Context of the evaluation.</typeparam>
    public interface IConditionEvaluator<in TContext>
    {
        /// <summary>
        ///     Evaluate whether the given condition is met.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="context">Context of the evaluation.</param>
        /// <returns>Returns true if the condition is satisfied; otherwise false.</returns>
        bool Evaluate(ICondition condition, TContext context);

        /// <summary>
        ///     Evaluates whether all conditions of the given conditional are met.
        /// </summary>
        /// <param name="conditional">The conditional to evaluate.</param>
        /// <param name="context">Context of the evaluation.</param>
        /// <returns>Returns true if all conditions are satisfied; otherwise false.</returns>
        bool Evaluate(IConditional conditional, TContext context);
    }
}
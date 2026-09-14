namespace Sagittaras.Conditions.Handlers
{
    /// <summary>
    ///     Handles the evaluation logic for <see cref="ConditionType"/>.
    /// </summary>
    /// <typeparam name="TSubject"></typeparam>
    public interface IConditionHandler<in TSubject>
    {
        /// <summary>
        ///     Evaluates the condition against the subject.
        /// </summary>
        /// <param name="condition">Condition to evaluate.</param>
        /// <param name="subject">Subject against which the condition is evaluated.</param>
        /// <returns>Returns true if the condition is satisfied; otherwise false.</returns>
        bool Evaluate(ICondition condition, TSubject subject);
    }
}
using System;

namespace Sagittaras.Conditions.Handlers
{
    /// <summary>
    ///     Condition handler delegating the evaluation to a function.
    /// </summary>
    /// <typeparam name="TSubject">Subject against which the condition is evaluated.</typeparam>
    internal class DelegateConditionHandler<TSubject> : IConditionHandler<TSubject>
    {
        /// <summary>
        ///     Function evaluating the condition against the subject.
        /// </summary>
        private readonly Func<ICondition, TSubject, bool> _evaluate;

        /// <summary>
        ///     Creates the handler delegating the evaluation to the function.
        /// </summary>
        /// <param name="evaluate">Function evaluating the condition against the subject.</param>
        public DelegateConditionHandler(Func<ICondition, TSubject, bool> evaluate)
        {
            _evaluate = evaluate;
        }

        /// <inheritdoc />
        public bool Evaluate(ICondition condition, TSubject subject)
        {
            return _evaluate(condition, subject);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Sagittaras.Conditions.Evaluation
{
    /// <summary>
    ///     Builds the <see cref="ConditionManager"/> from evaluators of different contexts.
    /// </summary>
    /// <remarks>
    ///     Each context type can be registered only once.
    /// </remarks>
    public class ConditionManagerBuilder
    {
        /// <summary>
        ///     Evaluators registered for each supported context type.
        /// </summary>
        private readonly Dictionary<Type, object> _evaluators = new();

        /// <summary>
        ///     Registers the evaluator responsible for evaluation of conditions in the given context.
        /// </summary>
        /// <param name="evaluator">Evaluator of the context.</param>
        /// <typeparam name="TContext">Context served by the evaluator.</typeparam>
        /// <returns>Returns the builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the context type already has a registered evaluator.</exception>
        public ConditionManagerBuilder WithEvaluator<TContext>(IConditionEvaluator<TContext> evaluator)
        {
            return !_evaluators.TryAdd(typeof(TContext), evaluator) 
                ? throw new ArgumentException($"Condition evaluator for context type `{typeof(TContext)}` is already registered.", nameof(evaluator)) 
                : this;
        }

        /// <summary>
        ///     Creates the manager from the current registrations.
        /// </summary>
        /// <remarks>
        ///     Registrations are copied, so further changes of the builder do not affect already created managers.
        /// </remarks>
        /// <returns>Returns the immutable manager.</returns>
        public ConditionManager Build()
        {
            return new ConditionManager(new Dictionary<Type, object>(_evaluators));
        }
    }
}

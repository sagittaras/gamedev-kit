using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sagittaras.Conditions.Evaluation
{
    /// <inheritdoc />
    /// <remarks>
    ///     Manager is immutable once created and can be safely shared. Use <see cref="ConditionManagerBuilder"/>
    ///     to register the evaluators and create the manager.
    /// </remarks>
    public class ConditionManager : IConditionManager
    {
        /// <summary>
        ///     Evaluators registered for each supported context type.
        /// </summary>
        /// <remarks>
        ///     Values are instances of <see cref="IConditionEvaluator{TContext}"/> of the context type used as the key.
        ///     The generic type cannot be expressed here, registration keeps the pairing valid.
        /// </remarks>
        private readonly Dictionary<Type, object> _evaluators;

        /// <summary>
        ///     Creates the manager with the registered evaluators.
        /// </summary>
        /// <param name="evaluators">Evaluators registered for each supported context type. Manager takes ownership of the dictionary.</param>
        internal ConditionManager(Dictionary<Type, object> evaluators)
        {
            _evaluators = evaluators;
        }

        /// <inheritdoc />
        public bool Evaluate<TContext>(ICondition condition, TContext context)
        {
            return GetEvaluator<TContext>().Evaluate(condition, context);
        }

        /// <inheritdoc />
        public bool Evaluate<TContext>(IConditional conditional, TContext context)
        {
            return GetEvaluator<TContext>().Evaluate(conditional, context);
        }

        /// <inheritdoc />
        public bool TryGetEvaluator<TContext>([MaybeNullWhen(false)] out IConditionEvaluator<TContext> evaluator)
        {
            if (!_evaluators.TryGetValue(typeof(TContext), out object? registered))
            {
                evaluator = null;
                return false;
            }

            evaluator = (IConditionEvaluator<TContext>)registered;
            return true;
        }

        /// <summary>
        ///     Gets the evaluator registered for the context type.
        /// </summary>
        /// <typeparam name="TContext">Context of the evaluation.</typeparam>
        /// <returns>Returns the evaluator registered for the context type.</returns>
        /// <exception cref="ConditionEvaluatorNotRegisteredException">Thrown when no evaluator is registered for the context type.</exception>
        private IConditionEvaluator<TContext> GetEvaluator<TContext>()
        {
            return !TryGetEvaluator(out IConditionEvaluator<TContext>? evaluator) 
                ? throw new ConditionEvaluatorNotRegisteredException(typeof(TContext), _evaluators.Keys) 
                : evaluator;
        }
    }
}

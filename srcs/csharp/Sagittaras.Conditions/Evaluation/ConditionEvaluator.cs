using System.Collections.Generic;
using Sagittaras.Conditions.Handlers;
using Sagittaras.Conditions.Targeting;

namespace Sagittaras.Conditions.Evaluation
{
    /// <summary>
    ///     Evaluates conditions by resolving their subject from the context and passing it to the handler
    ///     registered for the condition type.
    /// </summary>
    /// <remarks>
    ///     Evaluator is immutable once created and can be safely shared. Use <see cref="ConditionEvaluatorBuilder{TContext,TSubject}"/>
    ///     to register handlers and resolvers and create the evaluator.
    /// </remarks>
    /// <typeparam name="TContext">Context of the evaluation.</typeparam>
    /// <typeparam name="TSubject">Subject against which the conditions are evaluated.</typeparam>
    public class ConditionEvaluator<TContext, TSubject> : IConditionEvaluator<TContext>
    {
        /// <summary>
        ///     Handlers registered for each supported condition type.
        /// </summary>
        private readonly Dictionary<ConditionType, IConditionHandler<TSubject>> _handlers;

        /// <summary>
        ///     Resolvers registered for each supported condition target.
        /// </summary>
        private readonly Dictionary<ConditionTarget, ITargetResolver<TContext, TSubject>> _resolvers;

        /// <summary>
        ///     Creates the evaluator with the registered handlers and resolvers.
        /// </summary>
        /// <param name="handlers">Handlers registered for each supported condition type. Evaluator takes ownership of the dictionary.</param>
        /// <param name="resolvers">Resolvers registered for each supported condition target. Evaluator takes ownership of the dictionary.</param>
        internal ConditionEvaluator(
            Dictionary<ConditionType, IConditionHandler<TSubject>> handlers,
            Dictionary<ConditionTarget, ITargetResolver<TContext, TSubject>> resolvers)
        {
            _handlers = handlers;
            _resolvers = resolvers;
        }

        /// <inheritdoc />
        /// <remarks>
        ///     When the subject cannot be resolved from the context, the condition is not satisfied
        ///     regardless of <see cref="ICondition.Negated"/>, as it could not be evaluated at all.
        /// </remarks>
        /// <exception cref="ConditionHandlerNotRegisteredException">Thrown when no condition handler is registered for the condition type.</exception>
        /// <exception cref="TargetResolverNotRegisteredException">Thrown when no target resolver is registered for the condition target.</exception>
        public bool Evaluate(ICondition condition, TContext context)
        {
            if (!_handlers.TryGetValue(condition.ConditionType, out IConditionHandler<TSubject>? handler))
            {
                throw new ConditionHandlerNotRegisteredException(condition.ConditionType);
            }

            if (!_resolvers.TryGetValue(condition.Target, out ITargetResolver<TContext, TSubject>? resolver))
            {
                throw new TargetResolverNotRegisteredException(condition.Target);
            }

            if (!resolver.TryResolve(context, out TSubject? subject))
            {
                return false;
            }

            return handler.Evaluate(condition, subject) != condition.Negated;
        }

        /// <inheritdoc />
        public bool Evaluate(IConditional conditional, TContext context)
        {
            IReadOnlyList<ICondition> conditions = conditional.Conditions;

            // Indexed loop avoids enumerator allocation during frequent evaluation.
            for (int i = 0; i < conditions.Count; i++)
            {
                if (!Evaluate(conditions[i], context))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

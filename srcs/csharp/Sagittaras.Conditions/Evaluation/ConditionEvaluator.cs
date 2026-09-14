using System.Collections.Generic;
using Sagittaras.Conditions.Handlers;
using Sagittaras.Conditions.Targeting;

namespace Sagittaras.Conditions.Evaluation
{
    /// <summary>
    ///     Evaluates conditions by resolving their subject from the context and passing it to the handler
    ///     registered for the condition type.
    /// </summary>
    /// <typeparam name="TContext">Context of the evaluation.</typeparam>
    /// <typeparam name="TSubject">Subject against which the conditions are evaluated.</typeparam>
    public class ConditionEvaluator<TContext, TSubject> : IConditionEvaluator<TContext>
    {
        /// <summary>
        ///     Handlers registered for each supported condition type.
        /// </summary>
        private readonly Dictionary<ConditionType, IConditionHandler<TSubject>> _handlers = new();

        /// <summary>
        ///     Resolvers registered for each supported condition target.
        /// </summary>
        private readonly Dictionary<ConditionTarget, ITargetResolver<TContext, TSubject>> _resolvers = new();

        /// <summary>
        ///     Registers the handler responsible for evaluation of the condition type.
        /// </summary>
        /// <param name="type">Condition type served by the handler.</param>
        /// <param name="handler">Handler evaluating the condition type.</param>
        /// <exception cref="System.ArgumentException">Thrown when the condition type already has a handler.</exception>
        public void RegisterHandler(ConditionType type, IConditionHandler<TSubject> handler)
        {
            _handlers.Add(type, handler);
        }

        /// <summary>
        ///     Registers the resolver responsible for resolution of the condition target.
        /// </summary>
        /// <param name="target">Condition target served by the resolver.</param>
        /// <param name="resolver">Resolver finding the subject for the condition target.</param>
        /// <exception cref="System.ArgumentException">Thrown when the condition target already has a resolver.</exception>
        public void RegisterResolver(ConditionTarget target, ITargetResolver<TContext, TSubject> resolver)
        {
            _resolvers.Add(target, resolver);
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

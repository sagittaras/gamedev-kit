using System;
using System.Collections.Generic;
using Sagittaras.Conditions.Handlers;
using Sagittaras.Conditions.Targeting;

namespace Sagittaras.Conditions.Evaluation
{
    /// <summary>
    ///     Builds the <see cref="ConditionEvaluator{TContext,TSubject}"/> from registered handlers and resolvers.
    /// </summary>
    /// <remarks>
    ///     Registrations can be composed from multiple places, for example by extension methods of game modules.
    ///     Each condition type and condition target can be registered only once.
    /// </remarks>
    /// <typeparam name="TContext">Context of the evaluation.</typeparam>
    /// <typeparam name="TSubject">Subject against which the conditions are evaluated.</typeparam>
    public class ConditionEvaluatorBuilder<TContext, TSubject>
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
        /// <returns>Returns the builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the condition type already has a registered handler.</exception>
        public ConditionEvaluatorBuilder<TContext, TSubject> WithHandler(ConditionType type, IConditionHandler<TSubject> handler)
        {
            if (!_handlers.TryAdd(type, handler))
            {
                throw new ArgumentException($"Condition handler for condition type `{type}` is already registered.", nameof(type));
            }

            return this;
        }

        /// <summary>
        ///     Registers the evaluation function responsible for evaluation of the condition type.
        /// </summary>
        /// <param name="type">Condition type served by the function.</param>
        /// <param name="evaluate">Function returning true if the condition is satisfied by the subject; otherwise false.</param>
        /// <returns>Returns the builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the condition type already has a registered handler.</exception>
        public ConditionEvaluatorBuilder<TContext, TSubject> WithHandler(ConditionType type, Func<ICondition, TSubject, bool> evaluate)
        {
            return WithHandler(type, new DelegateConditionHandler<TSubject>(evaluate));
        }

        /// <summary>
        ///     Registers the resolver responsible for resolution of the condition target.
        /// </summary>
        /// <param name="target">Condition target served by the resolver.</param>
        /// <param name="resolver">Resolver finding the subject for the condition target.</param>
        /// <returns>Returns the builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the condition target already has a registered resolver.</exception>
        public ConditionEvaluatorBuilder<TContext, TSubject> WithResolver(ConditionTarget target, ITargetResolver<TContext, TSubject> resolver)
        {
            if (!_resolvers.TryAdd(target, resolver))
            {
                throw new ArgumentException($"Target resolver for condition target `{target}` is already registered.", nameof(target));
            }

            return this;
        }

        /// <summary>
        ///     Registers the resolution function responsible for resolution of the condition target.
        /// </summary>
        /// <remarks>
        ///     Returning null means the context has no such subject. Value type subjects cannot be null
        ///     and are therefore always resolved; implement <see cref="ITargetResolver{TContext,TSubject}"/>
        ///     when they need to be unresolved.
        /// </remarks>
        /// <param name="target">Condition target served by the function.</param>
        /// <param name="resolve">Function returning the subject from the context, or null if there is none.</param>
        /// <returns>Returns the builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the condition target already has a registered resolver.</exception>
        public ConditionEvaluatorBuilder<TContext, TSubject> WithResolver(ConditionTarget target, Func<TContext, TSubject?> resolve)
        {
            return WithResolver(target, new DelegateTargetResolver<TContext, TSubject>(resolve));
        }

        /// <summary>
        ///     Creates the evaluator from the current registrations.
        /// </summary>
        /// <remarks>
        ///     Registrations are copied, so further changes of the builder do not affect already created evaluators.
        /// </remarks>
        /// <returns>Returns the immutable evaluator.</returns>
        public ConditionEvaluator<TContext, TSubject> Build()
        {
            return new ConditionEvaluator<TContext, TSubject>(
                new Dictionary<ConditionType, IConditionHandler<TSubject>>(_handlers),
                new Dictionary<ConditionTarget, ITargetResolver<TContext, TSubject>>(_resolvers));
        }
    }
}

using System.Diagnostics.CodeAnalysis;

namespace Sagittaras.Conditions.Evaluation
{
    /// <summary>
    ///     Combines evaluators of different contexts into a single entry point for the condition evaluation.
    /// </summary>
    /// <remarks>
    ///     Evaluator is looked up by the type of the context, which is taken from the call site. Context
    ///     passed as its base type or interface therefore does not find an evaluator registered for the
    ///     derived type.
    /// </remarks>
    public interface IConditionManager
    {
        /// <summary>
        ///     Evaluates whether the given condition is met, using the evaluator registered for the context type.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="context">Context of the evaluation.</param>
        /// <typeparam name="TContext">Context of the evaluation.</typeparam>
        /// <returns>Returns true if the condition is satisfied; otherwise false.</returns>
        /// <exception cref="ConditionEvaluatorNotRegisteredException">Thrown when no evaluator is registered for the context type.</exception>
        bool Evaluate<TContext>(ICondition condition, TContext context);

        /// <summary>
        ///     Evaluates whether all conditions of the given conditional are met, using the evaluator
        ///     registered for the context type.
        /// </summary>
        /// <param name="conditional">The conditional to evaluate.</param>
        /// <param name="context">Context of the evaluation.</param>
        /// <typeparam name="TContext">Context of the evaluation.</typeparam>
        /// <returns>Returns true if all conditions are satisfied; otherwise false.</returns>
        /// <exception cref="ConditionEvaluatorNotRegisteredException">Thrown when no evaluator is registered for the context type.</exception>
        bool Evaluate<TContext>(IConditional conditional, TContext context);

        /// <summary>
        ///     Tries to get the evaluator registered for the context type.
        /// </summary>
        /// <remarks>
        ///     Useful for systems evaluating conditions repeatedly, as they can keep the evaluator
        ///     instead of looking it up for each evaluation.
        /// </remarks>
        /// <param name="evaluator">Evaluator registered for the context type, if there is one.</param>
        /// <typeparam name="TContext">Context of the evaluation.</typeparam>
        /// <returns>Returns true if the evaluator was found; otherwise false.</returns>
        bool TryGetEvaluator<TContext>([MaybeNullWhen(false)] out IConditionEvaluator<TContext> evaluator);
    }
}

using System.Diagnostics.CodeAnalysis;

namespace Sagittaras.Conditions.Targeting
{
    /// <summary>
    ///     Resolves the subject of evaluation for <see cref="ConditionTarget"/> from the evaluation context.
    /// </summary>
    /// <remarks>
    ///     Each resolver serves a single <see cref="ConditionTarget"/>, the same way as condition handler
    ///     serves a single <see cref="ConditionType"/>. New targets are added by registering new resolvers.
    /// </remarks>
    /// <typeparam name="TContext">Context of the evaluation.</typeparam>
    /// <typeparam name="TSubject">Subject against which the condition is evaluated.</typeparam>
    public interface ITargetResolver<in TContext, TSubject>
    {
        /// <summary>
        ///     Tries to resolve the subject from the evaluation context.
        /// </summary>
        /// <param name="context">Context of the evaluation.</param>
        /// <param name="subject">Resolved subject, if the context contains one.</param>
        /// <returns>
        ///     Returns true if the subject was resolved; otherwise false, for example when the context
        ///     has no such subject at the moment of evaluation.
        /// </returns>
        bool TryResolve(TContext context, [MaybeNullWhen(false)] out TSubject subject);
    }
}

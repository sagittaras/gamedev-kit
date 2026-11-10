using System;
using System.Diagnostics.CodeAnalysis;

namespace Sagittaras.Conditions.Targeting
{
    /// <summary>
    ///     Target resolver delegating the resolution to a function.
    /// </summary>
    /// <typeparam name="TContext">Context of the evaluation.</typeparam>
    /// <typeparam name="TSubject">Subject against which the condition is evaluated.</typeparam>
    internal class DelegateTargetResolver<TContext, TSubject> : ITargetResolver<TContext, TSubject>
    {
        /// <summary>
        ///     Function returning the subject from the context, or null if there is none.
        /// </summary>
        private readonly Func<TContext, TSubject?> _resolve;

        /// <summary>
        ///     Creates the resolver delegating the resolution to the function.
        /// </summary>
        /// <param name="resolve">Function returning the subject from the context, or null if there is none.</param>
        public DelegateTargetResolver(Func<TContext, TSubject?> resolve)
        {
            _resolve = resolve;
        }

        /// <inheritdoc />
        public bool TryResolve(TContext context, [MaybeNullWhen(false)] out TSubject subject)
        {
            TSubject? resolved = _resolve(context);
            if (resolved is null)
            {
                subject = default;
                return false;
            }

            subject = resolved;
            return true;
        }
    }
}

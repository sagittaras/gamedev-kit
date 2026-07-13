using System;

namespace Sagittaras.GuardClauses.Extensions
{
    /// <summary>
    ///     Provides boolean value guard clauses.
    /// </summary>
    public static class GuardClauseBooleanExtension
    {
        /// <summary>
        ///     Ensures that a given boolean value is not false.
        /// </summary>
        /// <param name="_">An instance of <see cref="IGuardClause"/>.</param>
        /// <param name="value">The value to guard.</param>
        /// <param name="message">Custom message to include in the exception.</param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the provided value is false.
        /// </exception>
        public static void False(this IGuardClause _, bool value, string? message = null)
        {
            if (!value)
            {
                throw new InvalidOperationException(message ?? "Value was expected to be true.");
            }
        }
    }
}
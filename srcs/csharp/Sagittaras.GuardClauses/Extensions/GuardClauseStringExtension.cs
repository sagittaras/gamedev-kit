using System;
using System.Linq;

namespace Sagittaras.GuardClauses.Extensions
{
    /// <summary>
    ///     Provides string value guard clauses.
    /// </summary>
    public static class GuardClauseStringExtension
    {
        /// <summary>
        ///     Ensures that a given string is neither null nor empty.
        /// </summary>
        /// <param name="_">An instance of <see cref="IGuardClause"/>.</param>
        /// <param name="input">The value to guard.</param>
        /// <param name="message">Custom message to include in the exception.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when the provided value is null or empty.
        /// </exception>
        public static void NullOrEmpty(this IGuardClause _, string? input, string? message = null)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException(message ?? "Value must not be null or empty.");
            }
        }

        /// <summary>
        ///     Ensures that a given string holds something other than white space.
        /// </summary>
        /// <param name="_">An instance of <see cref="IGuardClause"/>.</param>
        /// <param name="input">The value to guard.</param>
        /// <param name="message">Custom message to include in the exception.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when the provided value is null, empty or white space only.
        /// </exception>
        public static void NullOrWhiteSpace(this IGuardClause _, string? input, string? message = null)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException(message ?? "Value must not be null or white space.");
            }
        }

        /// <summary>
        ///     Ensures that a given string is a single word.
        /// </summary>
        /// <remarks>
        ///     For values that are later matched, split or spelled as one - an identifier, a key, a name the
        ///     input is recognized by. Passing null or an empty value is not what this clause is about, and
        ///     those get through; guard them with <see cref="NullOrWhiteSpace"/> beforehand.
        /// </remarks>
        /// <param name="_">An instance of <see cref="IGuardClause"/>.</param>
        /// <param name="input">The value to guard.</param>
        /// <param name="message">Custom message to include in the exception.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when the provided value holds a white space character.
        /// </exception>
        public static void WhiteSpaceInside(this IGuardClause _, string? input, string? message = null)
        {
            if (!string.IsNullOrEmpty(input) && input!.Any(char.IsWhiteSpace))
            {
                throw new ArgumentException(message ?? $"Value '{input}' must be a single word, without white space in it.");
            }
        }
    }
}

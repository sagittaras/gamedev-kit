using System;

namespace Sagittaras.GuardClauses.Extensions
{
    /// <summary>
    ///     Provides out-of-range guard clauses for integer values.
    /// </summary>
    public static class GuardClauseOutOfRangeExtension
    {
        /// <summary>
        ///     Ensures that an integer input value falls within the specified range.
        /// </summary>
        /// <param name="_">An instance of <see cref="IGuardClause"/> used to apply the guard clause.</param>
        /// <param name="input">The integer value to validate.</param>
        /// <param name="min">The minimum allowed value of the range, inclusive.</param>
        /// <param name="max">The maximum allowed value of the range, inclusive.</param>
        /// <param name="message">A custom message to include in the exception.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="input"/> value is less than <paramref name="min"/> or greater than <paramref name="max"/>.
        /// </exception>
        public static void OutOfRange(this IGuardClause _, int input, int min, int max, string? message = null)
        {
            if (input < min || input > max)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, message ?? $"Value must be between {min} and {max}");
            }
        }

        /// <inheritdoc cref="OutOfRange(IGuardClause,int,int,int,string?)" />
        public static void OutOfRange(this IGuardClause _, float input, float min, float max, string? message = null)
        {
            if (input < min || input > max)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, message ?? $"Value must be between {min} and {max}");
            }
        }
        
        /// <inheritdoc cref="OutOfRange(IGuardClause,int,int,int,string?)" />
        public static void OutOfRange(this IGuardClause _, double input, double min, double max, string? message = null)
        {
            if (input < min || input > max)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, message ?? $"Value must be between {min} and {max}");
            }
        }
    }
}
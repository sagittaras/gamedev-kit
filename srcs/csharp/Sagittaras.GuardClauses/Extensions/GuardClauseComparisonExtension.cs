using System;

namespace Sagittaras.GuardClauses.Extensions
{
    public static class GuardClauseComparisonExtension
    {
        /// <summary>
        ///     Ensures that a given value is less than a specified threshold.
        /// </summary>
        /// <param name="_">An instance of <see cref="IGuardClause"/> for method chaining.</param>
        /// <param name="input">The value to check.</param>
        /// <param name="threshold">The threshold value to compare against.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="input"/> is not less than the <paramref name="threshold"/>.
        /// </exception>
        public static void GreaterThan(this IGuardClause _, int input, int threshold)
        {
            if (input > threshold)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, $"Value must be less than {threshold}");
            }
        }

        /// <summary>
        ///     Ensures that a given value is greater than or equal to a specified threshold.
        /// </summary>
        /// <param name="_">An instance of <see cref="IGuardClause"/> for method chaining.</param>
        /// <param name="input">The value to check.</param>
        /// <param name="threshold">The threshold value to compare against.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the <paramref name="input"/> is less than the <paramref name="threshold"/>.
        /// </exception>
        public static void LessThan(this IGuardClause _, int input, int threshold)
        {
            if (input < threshold)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, $"Value must be greater than {threshold}");
            }
        }
    }
}
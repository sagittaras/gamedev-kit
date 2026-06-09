using System;

namespace Sagittaras.GuardClauses.Extensions
{
    public static class GuardClauseComparisonExtension
    {
        /// <summary>
        ///     Ensures that a given value is greater than a specified threshold.
        /// </summary>
        /// <param name="_">An instance of <see cref="IGuardClause"/> for method chaining.</param>
        /// <param name="input">The value to check.</param>
        /// <param name="threshold">The threshold value to compare against.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="input"/> is not greater than the <paramref name="threshold"/>.
        /// </exception>
        public static void GreaterThan(this IGuardClause _, int input, int threshold)
        {
            if (input > threshold)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, $"Value must be greater than {threshold}");
            }
        }
    }
}
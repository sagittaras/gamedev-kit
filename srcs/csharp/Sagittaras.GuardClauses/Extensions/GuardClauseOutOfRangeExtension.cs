using System;

namespace Sagittaras.GuardClauses.Extensions
{
    public static class GuardClauseOutOfRangeExtension
    {
        /// <summary>
        ///     Ensures that an integer input value falls within the specified range.
        /// </summary>
        /// <param name="_">An instance of <see cref="IGuardClause"/> used to apply the guard clause.</param>
        /// <param name="input">The integer value to validate.</param>
        /// <param name="min">The minimum allowed value of the range, inclusive.</param>
        /// <param name="max">The maximum allowed value of the range, inclusive.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="input"/> value is less than <paramref name="min"/> or greater than <paramref name="max"/>.
        /// </exception>
        public static void OutOfRange(this IGuardClause _, int input, int min, int max)
        {
            if (input < min || input > max)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, $"Value must be between {min} and {max}");
            }
        }
    }
}
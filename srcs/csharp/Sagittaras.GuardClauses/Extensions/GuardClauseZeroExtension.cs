using System;

namespace Sagittaras.GuardClauses.Extensions
{
    public static class GuardClauseZeroExtension
    {
        /// <summary>
        ///     Ensures that the provided integer input is not zero; otherwise, throws a <see cref="DivideByZeroException"/>.
        /// </summary>
        /// <param name="_">The guard clause instance. This parameter is often provided implicitly and is not directly used in the method logic.</param>
        /// <param name="input">The integer input to validate against a division by zero.</param>
        /// <exception cref="DivideByZeroException">Thrown when the input value is zero, as division by zero is not allowed.</exception>
        public static void DivisionByZero(this IGuardClause _, int input)
        {
            if (input == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero.");
            }
        }

        /// <summary>
        ///     Ensures that the provided integer input is not less than zero; otherwise, throws an <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name="_">The guard clause instance. This parameter is often provided implicitly and is not directly used in the method logic.</param>
        /// <param name="input">The integer input to validate to ensure it is not less than zero.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the input value is less than zero.</exception>
        public static void LessThanZero(this IGuardClause _, float input)
        {
            if (input < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, "Value must be greater than zero.");
            }
        }
    }
}
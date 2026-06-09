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
    }
}
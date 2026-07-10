using System;

namespace Sagittaras.GuardClauses.Extensions
{
    public static class GuardClauseBooleanExtension
    {
        public static void False(this IGuardClause _, bool value, string? message = null)
        {
            if (!value)
            {
                throw new InvalidOperationException(message ?? "Value was expected to be true.");
            }
        }
    }
}
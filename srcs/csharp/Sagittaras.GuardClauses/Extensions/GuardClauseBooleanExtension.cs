using System;

namespace Sagittaras.GuardClauses.Extensions
{
    public static class GuardClauseBooleanExtension
    {
        public static void False(this IGuardClause _, bool value)
        {
            if (!value)
            {
                throw new InvalidOperationException("Value was expected to be true.");
            }
        }
    }
}
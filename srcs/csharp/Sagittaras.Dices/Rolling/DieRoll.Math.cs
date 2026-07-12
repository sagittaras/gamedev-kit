#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Dices.Rolling
{
    // Defines mathematical operations, mainly for altering the base value.
    public readonly partial struct DieRoll
    {
        public static DieRoll operator +(DieRoll a, int b)
        {
            return new DieRoll(a.BaseValue + b, a.DieSides);
        }
        
        public static DieRoll operator -(DieRoll a, int b)
        {
            return new DieRoll(a.BaseValue - b, a.DieSides);
        }
        
        public static DieRoll operator *(DieRoll a, int b)
        {
            return new DieRoll(a.BaseValue * b, a.DieSides);
        }
        
        public static DieRoll operator /(DieRoll a, int b)
        {
            Guard.Against.DivisionByZero(b);

            return new DieRoll(a.BaseValue / b, a.DieSides);
        }
    }
}
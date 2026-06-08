using System;

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
            return b == 0 
                ? throw new DivideByZeroException("Cannot divide DieRoll by zero.") 
                : new DieRoll(a.BaseValue / b, a.DieSides);
        }
    }
}
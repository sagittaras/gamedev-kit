namespace Sagittaras.Dices.Probability
{
    // Defines equality and comparison operators.
    public readonly partial struct Chance
    {
        public static bool operator ==(Chance a, Chance b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(Chance a, Chance b)
        {
            return a.Value != b.Value;
        }
        
        public static bool operator >(Chance a, Chance b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <(Chance a, Chance b)
        {
            return a.Value < b.Value;
        }

        public static bool operator >=(Chance a, Chance b)
        {
            return a.Value >= b.Value;
        }

        public static bool operator <=(Chance a, Chance b)
        {
            return a.Value <= b.Value;
        }
    }
}
namespace Sagittaras.Dices.Probability
{
    // Defines mathematical operations for Chance.
    public readonly partial struct Chance
    {
        #region Chance with Chance

        public static Chance operator +(Chance a, Chance b)
        {
            return new Chance(a.Value + b.Value);
        }

        public static Chance operator -(Chance a, Chance b)
        {
            return new Chance(a.Value - b.Value);
        }

        #endregion

        #region Left-Side Chance

        #region Integer

        public static Chance operator +(Chance a, int b)
        {
            return new Chance(a.Value + b);
        }

        public static Chance operator -(Chance a, int b)
        {
            return new Chance(a.Value - b);
        }

        #endregion
        
        #endregion
    }
}
namespace Sagittaras.Dices.Probability
{
    // Defines an implicit conversion between numeric types and Chance.
    public readonly partial struct Chance
    {
        #region From Numerics

        public static implicit operator Chance(int value)
        {
            return new Chance(value);
        }

        public static implicit operator Chance(float value)
        {
            return new Chance(value);
        }
        
        public static implicit operator Chance(double value)
        {
            return new Chance(value);
        }
        
        #endregion

        #region To Numerics

        public static implicit operator int(Chance chance)
        {
            return chance.Value;
        }

        public static implicit operator float(Chance chance)
        {
            return chance.Value / (float)Factor;
        }

        public static implicit operator double(Chance chance)
        {
            return chance.Value / (double)Factor;
        }

        #endregion
    }
}
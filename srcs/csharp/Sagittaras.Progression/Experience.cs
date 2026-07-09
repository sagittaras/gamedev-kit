namespace Sagittaras.Progression
{
    /// <summary>
    ///     Represents a specific value in a player's progression towards the next level.
    /// </summary>
    public readonly partial struct Experience
    {
        /// <summary>
        ///     A zero experience value.
        /// </summary>
        public static readonly Experience Zero = new(0);
        
        public Experience(int value)
        {
            Value = value;
        }
        
        /// <summary>
        ///     A current experience value expressed by this value type.
        /// </summary>
        public int Value { get; }

        #region Implicit Conversion

        public static implicit operator Experience(int value)
        {
            return new Experience(value);
        }

        public static implicit operator int(Experience xp)
        {
            return xp.Value;
        }

        #endregion
        
        #region Operators

        public static bool operator ==(Experience a, Experience b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(Experience a, Experience b)
        {
            return a.Value != b.Value;
        }

        public static bool operator >(Experience a, Experience b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <(Experience a, Experience b)
        {
            return a.Value < b.Value;
        }

        public static bool operator >=(Experience a, Experience b)
        {
            return a.Value >= b.Value;
        }

        public static bool operator <=(Experience a, Experience b)
        {
            return a.Value <= b.Value;
        }

        #endregion
    }
}
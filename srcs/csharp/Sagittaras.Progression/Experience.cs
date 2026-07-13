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
        
        /// <summary>
        ///     Creates a new experience value.
        /// </summary>
        /// <param name="value">Value of the experience.</param>
        public Experience(int value)
        {
            Value = value;
        }
        
        /// <summary>
        ///     A current experience value expressed by this value type.
        /// </summary>
        public int Value { get; }

        #region Implicit Conversion
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        
        public static implicit operator Experience(int value)
        {
            return new Experience(value);
        }

        public static implicit operator int(Experience xp)
        {
            return xp.Value;
        }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
    }
}
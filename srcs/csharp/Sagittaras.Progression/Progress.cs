namespace Sagittaras.Progression
{
    /// <summary>
    ///     Represents a player's progression towards the next level.
    /// </summary>
    public readonly struct Progress
    {
        /// <summary>
        ///     Creates a new progression instance.
        /// </summary>
        /// <param name="threshold">Threshold required to reach to advance to the next level.</param>
        /// <param name="value">Current value of experience.</param>
        public Progress(Experience threshold, Experience value)
        {
            Value = value;
            Threshold = threshold;
        }

        /// <summary>
        ///     Creates a new progression instance.
        /// </summary>
        /// <param name="threshold">Threshold required to reach to advance to the next level.</param>
        public Progress(Experience threshold) : this(threshold, Experience.Zero)
        {
        }
        
        /// <summary>
        ///     Current value of experience.
        /// </summary>
        public Experience Value { get; }
        
        /// <summary>
        ///     Threshold value of experience required to reach the next level.
        /// </summary>
        public Experience Threshold { get; }

        /// <summary>
        ///     Indicates whether the progression has reached the threshold.
        /// </summary>
        public bool Reached => Value >= Threshold;
        
        public static Progress operator +(Progress progress, Experience experience)
        {
            return new Progress(progress.Threshold, progress.Value + experience);
        }
        
        public static Progress operator -(Progress progress, Experience experience)
        {
            return new Progress(progress.Threshold, progress.Value - experience);
        }
    }
}
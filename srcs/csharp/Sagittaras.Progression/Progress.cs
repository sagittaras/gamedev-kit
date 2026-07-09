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
        /// <param name="current">Current value of experience.</param>
        public Progress(Experience threshold, Experience current)
        {
            Current = current;
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
        public Experience Current { get; }
        
        /// <summary>
        ///     Threshold value of experience required to reach the next level.
        /// </summary>
        public Experience Threshold { get; }

        /// <summary>
        ///     Indicates whether the progression has reached the threshold.
        /// </summary>
        public bool Reached => Current >= Threshold;
        
        public static Progress operator +(Progress progress, Experience experience)
        {
            return new Progress(progress.Threshold, progress.Current + experience);
        }
        
        public static Progress operator -(Progress progress, Experience experience)
        {
            return new Progress(progress.Threshold, progress.Current - experience);
        }
    }
}
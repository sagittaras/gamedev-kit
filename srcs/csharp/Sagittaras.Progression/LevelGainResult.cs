namespace Sagittaras.Progression
{
    /// <summary>
    ///     Result of <see cref="Level.Gain"/> operation.
    /// </summary>
    public readonly struct LevelGainResult
    {
        /// <summary>
        ///     Creates a new instance of <see cref="LevelGainResult"/>.
        /// </summary>
        /// <param name="level">Newest instance of level value.</param>
        /// <param name="levelsGained">Number of levels gained during the <see cref="Level.Gain"/> call.</param>
        public LevelGainResult(Level level, int levelsGained)
        {
            Level = level;
            LevelsGained = levelsGained;
        }
        
        /// <summary>
        ///     Instance of a newly gained level.
        /// </summary>
        public Level Level { get; }
        
        /// <summary>
        ///     Number of levels gained.
        /// </summary>
        public int LevelsGained { get; }
        
        /// <summary>
        ///     Indicates whether there was a change to the level.
        /// </summary>
        public bool LeveledUp => LevelsGained > 0;
        
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator Level(LevelGainResult result)
        {
            return result.Level;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
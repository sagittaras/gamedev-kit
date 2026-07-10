namespace Sagittaras.Progression
{
    /// <summary>
    ///     Result of <see cref="Level.Gain"/> operation.
    /// </summary>
    public readonly struct LevelGainResult
    {
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
        
        public static implicit operator Level(LevelGainResult result)
        {
            return result.Level;
        }
    }
}
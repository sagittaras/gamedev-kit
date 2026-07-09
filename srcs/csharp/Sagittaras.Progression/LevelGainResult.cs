namespace Sagittaras.Progression
{
    /// <summary>
    ///     Result of <see cref="Level.Gain"/> operation.
    /// </summary>
    public readonly struct LevelGainResult
    {
        public LevelGainResult(Level original, Level current, int levelsGained)
        {
            Original = original;
            Current = current;
            LevelsGained = levelsGained;
        }
        
        /// <summary>
        ///     Instance of a level before the operation.
        /// </summary>
        public Level Original { get; }
        
        /// <summary>
        ///     Instance of a newly gained level.
        /// </summary>
        public Level Current { get; }
        
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
            return result.Current;
        }
    }
}
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Progression
{
    /// <summary>
    ///     Carries a value representing the current level and the progress towards the next one. 
    /// </summary>
    public readonly struct Level
    {
        private const int DefaultMinValue = 1;
        
        /// <summary>
        ///     A default minimal level value.
        /// </summary>
        public static readonly Level MinValue = new(DefaultMinValue);
        
        /// <summary>
        ///     The experience formula used to calculate the experience required for level progression.
        /// </summary>
        private readonly IExperienceFormula _formula;

        /// <summary>
        ///     Creates a new <see cref="Level"/> instance.
        /// </summary>
        /// <param name="level">Current level value.</param>
        /// <param name="formula">Formula used to calculate the threshold for level progression.</param>
        /// <param name="overflow">Overflow experience value from the previous level.</param>
        public Level(int level, IExperienceFormula? formula = null, Experience? overflow = null)
        {
            Guard.Against.LessThan(level, DefaultMinValue);
            
            _formula = formula ?? IExperienceFormula.Default;
            Value = level;
            Progress = new Progress(_formula.Calculate(level), overflow ?? Experience.Zero);
        }

        private Level(Level other, Progress progress)
        {
            _formula = other._formula;

            Value = other.Value;
            Progress = progress;
        }

        /// <summary>
        ///     Current level value.
        /// </summary>
        public int Value { get; }

        /// <inheritdoc cref="Sagittaras.Progression.Progress" />
        public Progress Progress { get; }

        /// <summary>
        ///     Adds a new experience gain to the current level.
        /// </summary>
        /// <param name="experience">Experience added to the current level.</param>
        /// <returns>
        ///     Struct containing the result of the operation, indicating whether the level was gained.
        /// </returns>
        public LevelGainResult Gain(Experience experience)
        {
            int levelsGained = 0;
            Level current = this + experience;

            while (current.Progress.Reached)
            {
                current = current.LevelUp();
                levelsGained++;
            }

            return new LevelGainResult(this, current, levelsGained);
        }

        /// <summary>
        ///     Makes a new instance of level.
        /// </summary>
        /// <returns>New instance of level with corresponding progression value.</returns>
        private Level LevelUp()
        {
            Guard.Against.False(Progress.Reached, $"Player did not reach the threshold to level up [{Progress.Current} / {Progress.Threshold} XP]");

            Experience overflow = Progress.Current - Progress.Threshold;
            return new Level(Value + 1, _formula, overflow);
        }

        public static Level operator +(Level a, Experience b)
        {
            return new Level(a, a.Progress + b);
        }
    }
}
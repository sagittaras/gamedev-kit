using System;

namespace Sagittaras.Progression.Formulas
{
    /// <summary>
    ///     Represents the default formula used to calculate the experience required to level up in the progression system.
    /// </summary>
    /// <remarks>
    ///     This formula calculates the experience based on a base value and an exponential multiplier. It provides
    ///     a structured approach for defining progression curves within the game.
    /// </remarks>
    public sealed class ExponentialScalingFormula : IExperienceFormula
    {
        private const float DefaultMultiplier = 1.25f;
        private const int DefaultBaseValue = 100;

        /// <summary>
        ///     Exponential multiplier applied per level.
        /// </summary>
        private readonly float _multiplier;
        
        /// <summary>
        ///     Experience points required to advance from level 1 to level 2, as a base value.
        /// </summary>
        private readonly int _baseValue;
        
        public ExponentialScalingFormula(float multiplier = DefaultMultiplier, int baseValue = DefaultBaseValue)
        {
            _multiplier = multiplier;
            _baseValue = baseValue;
        }

        /// <inheritdoc />
        public Experience Calculate(int currentLevel)
        {
            return currentLevel == 1
                ? _baseValue
                : (int) (_baseValue * Math.Pow(_multiplier, currentLevel - 1));
        }
    }
}
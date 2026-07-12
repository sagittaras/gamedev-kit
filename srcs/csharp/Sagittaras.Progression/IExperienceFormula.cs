using Sagittaras.Progression.Formulas;

namespace Sagittaras.Progression
{
    /// <summary>
    ///     A formula that calculates the experience required to reach a certain level.
    /// </summary>
    public interface IExperienceFormula
    {
        static IExperienceFormula Default = new ExponentialScalingFormula();
        
        /// <summary>
        ///     Calculates the experience required to reach the next level.
        /// </summary>
        /// <param name="currentLevel">Current player's level.</param>
        /// <returns>Threshold required to reach the next level.</returns>
        Experience Calculate(int currentLevel);
    }
}
using Sagittaras.Dices.Probability;

namespace Sagittaras.Dices.Extensions
{
    public static class ChanceExtension
    {
        /// <summary>
        ///     Evaluates whether a given chance succeeds, based on the internal probability logic.
        /// </summary>
        /// <param name="chance">The chance to be evaluated, represented by a <see cref="Chance"/> instance.</param>
        /// <returns>Returns true if the chance succeeds; otherwise, false.</returns>
        public static bool Try(this Chance chance)
        {
            return DiceBag.TryChance(chance);
        }
    }
}
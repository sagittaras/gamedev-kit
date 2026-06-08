using Sagittaras.Dices.Probability;
using Sagittaras.Dices.Rolling;

namespace Sagittaras.Dices.Extensions
{
    internal static class DiceBagAdapterExtension
    {
        /// <summary>
        ///     Generates a random integer value between 0 and the specified chance value (inclusive).
        /// </summary>
        /// <param name="adapter">The dice bag adapter used to generate the random integer value.</param>
        /// <param name="chance">Represents the upper bound (inclusive) of the randomization range.</param>
        /// <returns>A random integer value between 0 and the value of the specified chance (inclusive).</returns>
        internal static int NextChance(this IDiceBagAdapter adapter, Chance chance)
        {
            return adapter.Next(0, chance.Value + 1);
        }

        /// <summary>
        ///     Generates a random integer value representing the outcome of rolling a die with the specified number of sides.
        /// </summary>
        /// <param name="adapter">The dice bag adapter used to generate the random integer value.</param>
        /// <param name="dieRoll">The die roll configuration, including the number of sides on the die.</param>
        /// <returns>A random integer value between 1 and the number of die sides (inclusive).</returns>
        internal static int NextDieSide(this IDiceBagAdapter adapter, DieRoll dieRoll)
        {
            return adapter.Next(DieRoll.MinDieSides, dieRoll.DieSides + 1);
        }
    }
}
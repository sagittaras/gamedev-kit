using Sagittaras.Dices.Probability;

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
    }
}
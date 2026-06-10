using Sagittaras.Dices.Rolling;

namespace Sagittaras.Dices.Extensions
{
    internal static class DiceBagAdapterExtension
    {
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
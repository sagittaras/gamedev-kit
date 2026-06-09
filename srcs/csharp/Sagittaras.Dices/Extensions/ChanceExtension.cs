using Sagittaras.Dices.Probability;

namespace Sagittaras.Dices.Extensions
{
    public static class ChanceExtension
    {
        /// <inheritdoc cref="DiceBag.Try" />
        public static bool Try(this Chance chance, IDiceBag? diceBag = null)
        {
            return (diceBag ?? DiceBag.Instance).Try(chance);
        }

        /// <summary>
        ///     Picks one of the two provided elements based on the given chance.
        /// </summary>
        /// <param name="chance">The probability to select the first element.</param>
        /// <param name="a">The first element to be picked.</param>
        /// <param name="b">The second element to be picked.</param>
        /// <param name="diceBag">An optional custom dice bag for random number generation. If not provided, the default instance is used.</param>
        /// <typeparam name="T">The type of elements to pick from.</typeparam>
        /// <returns>Returns the selected element based on the weighted chance.</returns>
        public static T WeightedPick<T>(this Chance chance, T a, T b, IDiceBag? diceBag = null)
        {
            return (diceBag ?? DiceBag.Instance).WeightedPick(chance, a, b);
        }

        /// <summary>
        ///     Normalizes the current instance of <see cref="Chance"/> based on the provided probability sum.
        /// </summary>
        /// <param name="chance">The <see cref="Chance"/> instance to normalize.</param>
        /// <param name="probabilitySum">The total sum of probabilities used for normalization.</param>
        /// <returns>A new <see cref="Chance"/> instance representing the normalized probability.</returns>
        public static Chance Normalize(this Chance chance, int probabilitySum)
        {
            if (probabilitySum <= Chance.MaxValue)
            {
                return chance;
            }

            double value = (double)chance.Value / probabilitySum * Chance.Factor;
            return new Chance(value);
        }
    }
}
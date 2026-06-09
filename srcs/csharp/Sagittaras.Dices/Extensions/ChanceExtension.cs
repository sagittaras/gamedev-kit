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
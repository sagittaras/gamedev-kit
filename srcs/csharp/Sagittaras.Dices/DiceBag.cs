using Sagittaras.Dices.Extensions;
using Sagittaras.Dices.Probability;

namespace Sagittaras.Dices
{
    /// <summary>
    ///     Provides functionality for rolling randomness-based probabilities and values, using various dice logic.
    /// </summary>
    public class DiceBag
    {
        private static DiceBag Instance { get; } = new();

        /// <summary>
        ///     Adapter providing the randomization mechanism for the dice bag.
        /// </summary>
        private IDiceBagAdapter Adapter { get; set; } = IDiceBagAdapter.Default;

        /// <summary>
        ///     Sets a new adapter that will be used to determine the behavior of the dice bag.
        /// </summary>
        /// <param name="adapter">
        ///     The implementation of <see cref="IDiceBagAdapter"/> that will define custom randomization logic
        ///     for the <see cref="DiceBag"/>.
        /// </param>
        public static void SetAdapter(IDiceBagAdapter adapter)
        {
            Instance.Adapter = adapter;
        }

        /// <summary>
        ///     Reseeds the underlying random number generator used within the dice bag,
        ///     generating a new sequence of random numbers by altering the adapter's internal state.
        /// </summary>
        public static void Reseed()
        {
            Instance.Adapter.Reseed();
        }

        /// <summary>
        ///     Evaluates whether a given chance succeeds based on the random probability mechanism of the dice bag.
        /// </summary>
        /// <param name="chance">The probability, represented as a <see cref="Chance"/> instance, to evaluate against.</param>
        /// <returns>Returns <c>true</c> if the random roll is within the provided chance's value; otherwise, <c>false</c>.</returns>
        public static bool TryChance(Chance chance)
        {
            return Instance.Adapter.NextChance(Chance.Max) <= chance.Value;
        }
    }
}
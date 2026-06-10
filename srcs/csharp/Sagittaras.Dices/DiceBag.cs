using Sagittaras.Dices.Extensions;
using Sagittaras.Dices.Probability;
using Sagittaras.Dices.Ranges;
using Sagittaras.Dices.Rolling;

namespace Sagittaras.Dices
{
    /// <summary>
    ///     Provides functionality for rolling randomness-based probabilities and values, using various dice logic.
    /// </summary>
    public class DiceBag : IDiceBag
    {
        public DiceBag()
        {
        }

        public DiceBag(IDiceBagAdapter adapter)
        {
            Adapter = adapter;
        }

        /// <summary>
        ///     Represents a globally shared instance of the dice bag, providing a single point of access for randomization operations.
        /// </summary>
        public static IDiceBag Instance { get; set; } = new DiceBag();
        
        /// <summary>
        ///     Adapter providing the randomization mechanism for the dice bag.
        /// </summary>
        private IDiceBagAdapter Adapter { get; set; } = IDiceBagAdapter.Default;

        /// <summary>
        ///     Reseeds the underlying random number generator used within the dice bag,
        ///     generating a new sequence of random numbers by altering the adapter's internal state.
        /// </summary>
        public void Reseed()
        {
            Adapter.Reseed();
        }

        /// <summary>
        ///     Evaluates whether a given chance succeeds based on the random probability mechanism of the dice bag.
        /// </summary>
        /// <param name="chance">The probability, represented as a <see cref="Chance"/> instance, to evaluate against.</param>
        /// <returns>Returns <c>true</c> if the random roll is within the provided chance's value; otherwise, <c>false</c>.</returns>
        public bool Try(Chance chance)
        {
            return Next(Chance.Range) <= chance.Value;
        }

        /// <summary>
        ///     Generates a random integer within the specified range using the configured adapter.
        /// </summary>
        /// <param name="range">
        ///     The inclusive range, represented by a <see cref="RandomRange"/>,
        ///     within which the random number will be generated.
        /// </param>
        /// <returns>A randomly generated integer within the specified range.</returns>
        public int Next(RandomRange range)
        {
            return Adapter.Next(range.Min, range.Max + 1);
        }

        /// <summary>
        ///     Rolls multi-side dice as specified by the given die roll configuration and calculates the resulting value.
        /// </summary>
        /// <param name="dieRoll">
        ///     The configuration for the dice roll, including the number of sides on the die and any base value to add
        ///     to the result.
        /// </param>
        /// <returns>The resulting integer value after performing the dice roll calculation, including the base value.</returns>
        public int Roll(DieRoll dieRoll)
        {
            return Adapter.NextDieSide(dieRoll) + dieRoll.BaseValue;
        }
    }
}
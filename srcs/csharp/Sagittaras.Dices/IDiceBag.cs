using Sagittaras.Dices.Probability;
using Sagittaras.Dices.Ranges;
using Sagittaras.Dices.Rolling;

namespace Sagittaras.Dices
{
    /// <summary>
    ///     Provides functionality for rolling randomness-based probabilities and values, using various dice logic.
    /// </summary>
    public interface IDiceBag
    {
        /// <summary>
        ///     Reseeds the underlying random number generator used within the dice bag,
        ///     generating a new sequence of random numbers by altering the adapter's internal state.
        /// </summary>
        void Reseed();
        
        /// <summary>
        ///     Evaluates whether a given chance succeeds based on the random probability mechanism of the dice bag.
        /// </summary>
        /// <param name="chance">The probability, represented as a <see cref="Chance"/> instance, to evaluate against.</param>
        /// <returns>Returns <c>true</c> if the random roll is within the provided chance's value; otherwise, <c>false</c>.</returns>
        bool Try(Chance chance);

        /// <summary>
        ///     Generates a random integer within the specified range using the configured adapter.
        /// </summary>
        /// <param name="range">
        ///     The inclusive range, represented by a <see cref="RandomRange"/>,
        ///     within which the random number will be generated.
        /// </param>
        /// <returns>A randomly generated integer within the specified range.</returns>
        int Next(RandomRange range);

        /// <summary>
        ///     Rolls multi-side dice as specified by the given die roll configuration and calculates the resulting value.
        /// </summary>
        /// <param name="dieRoll">
        ///     The configuration for the dice roll, including the number of sides on the die and any base value to add
        ///     to the result.
        /// </param>
        /// <returns>The resulting integer value after performing the dice roll calculation, including the base value.</returns>
        int Roll(DieRoll dieRoll);
    }
}
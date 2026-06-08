using Sagittaras.Dices.Extensions;

namespace Sagittaras.Dices.Probability
{
    // Describes a static API for the Chance.
    public readonly partial struct Chance
    {
        /// <summary>
        ///     Simulates a coin flip between two possible values and returns one of them based on a 50% probability.
        /// </summary>
        /// <typeparam name="T">The type of the values to be returned by the coin flip.</typeparam>
        /// <param name="a">The value to return if the coin flip is successful on the first option.</param>
        /// <param name="b">The value to return if the coin flip is successful on the second option.</param>
        /// <returns>
        ///     Returns either <paramref name="a"/> or <paramref name="b"/>, with a 50% probability of selecting each.
        /// </returns>
        public static T FlipCoin<T>(T a, T b)
        {
            return Half.Try() ? a : b;
        }
    }
}
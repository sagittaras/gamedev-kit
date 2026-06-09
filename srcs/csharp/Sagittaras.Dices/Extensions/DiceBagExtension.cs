using Sagittaras.Dices.Probability;

namespace Sagittaras.Dices.Extensions
{
    public static class DiceBagExtension
    {
        /// <summary>
        ///     Simulates a coin flip between two possible values and returns one of them based on a 50% probability.
        /// </summary>
        /// <typeparam name="T">The type of the values to be returned by the coin flip.</typeparam>
        /// <param name="diceBag">The dice bag used for generating the random coin flip outcome.</param>
        /// <param name="a">The value to return if the coin flip is successful on the first option.</param>
        /// <param name="b">The value to return if the coin flip is successful on the second option.</param>
        /// <returns>
        ///     Returns either <paramref name="a"/> or <paramref name="b"/>, with a 50% probability of selecting each.
        /// </returns>
        public static T FlipCoin<T>(this IDiceBag diceBag, T a, T b)
        {
            return Chance.Half.Try(diceBag) ? a : b;
        }
        
        /// <summary>
        ///     Generates a new random <see cref="Chance"/> value within the predefined range.
        /// </summary>
        /// <returns>
        ///     Returns a randomly generated <see cref="Chance"/> value between <see cref="Chance.MinValue"/> and <see cref="Chance.MaxValue"/>.
        /// </returns>
        public static Chance NextChance(this IDiceBag diceBag)
        {
            return diceBag.Next(Chance.Range);
        }
    }
}
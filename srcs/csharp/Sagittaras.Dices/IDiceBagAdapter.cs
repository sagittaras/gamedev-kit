using Sagittaras.Dices.Adapters;

namespace Sagittaras.Dices
{
    /// <summary>
    ///     Allows for providing custom randomization algorithms for the <see cref="DiceBag"/> class.
    /// </summary>
    /// <remarks>
    ///     Adapter can be used to provide a standard Random class or custom predictive algorithms
    ///     for test cases.
    /// </remarks>
    public interface IDiceBagAdapter
    {
        /// <summary>
        ///     Represents the default implementation of the <see cref="IDiceBagAdapter"/> interface
        ///     used to generate random numbers within the <see cref="DiceBag"/> class.
        /// </summary>
        /// <remarks>
        ///     The default adapter is an instance of <see cref="SystemRandomAdapter"/>, which uses
        ///     the standard <see cref="System.Random"/> class for number generation.
        /// </remarks>
        static readonly IDiceBagAdapter Default = new SystemRandomAdapter();
        
        /// <summary>
        ///     Generates a random integer within the specified range.
        /// </summary>
        /// <param name="min">The inclusive lower bound of the random number.</param>
        /// <param name="max">The exclusive upper bound of the random number.</param>
        /// <returns>A randomly generated integer within the range [min, max).</returns>
        int Next(int min, int max);

        /// <summary>
        ///     Reseeds the underlying random number generator to produce a new sequence
        ///     of random numbers, enabling changes in the algorithm's state or behavior.
        /// </summary>
        void Reseed();
    }
}
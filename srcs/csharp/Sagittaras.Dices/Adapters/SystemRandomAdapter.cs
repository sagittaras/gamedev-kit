using System;

namespace Sagittaras.Dices.Adapters
{
    /// <summary>
    ///     Provides a default implementation of <see cref="IDiceBagAdapter"/> using <see cref="System.Random"/> class.
    /// </summary>
    public class SystemRandomAdapter : IDiceBagAdapter
    {
        private Random Random { get; set; } = CreateRandom();

        /// <inheritdoc />
        public int Next(int min, int max)
        {
            return Random.Next(min, max);
        }

        /// <inheritdoc />
        public void Reseed()
        {
            Random = CreateRandom();
        }
        
        private static Random CreateRandom()
        {
            return new Random(Guid.NewGuid().GetHashCode());
        }
    }
}
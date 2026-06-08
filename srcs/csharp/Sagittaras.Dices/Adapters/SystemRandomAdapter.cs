using System;

namespace Sagittaras.Dices.Adapters
{
    /// <summary>
    ///     Provides a default implementation of <see cref="IDiceBagAdapter"/> using <see cref="Random"/> class.
    /// </summary>
    public class SystemRandomAdapter : IDiceBagAdapter
    {
        private Random _random = CreateRandom();

        /// <inheritdoc />
        public int Next(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <inheritdoc />
        public void Reseed()
        {
            _random = CreateRandom();
        }
        
        private static Random CreateRandom()
        {
            return new Random(Guid.NewGuid().GetHashCode());
        }
    }
}
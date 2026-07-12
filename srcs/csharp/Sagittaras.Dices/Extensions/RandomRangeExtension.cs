using Sagittaras.Dices.Ranges;

namespace Sagittaras.Dices.Extensions
{
    /// <summary>
    ///     Provides extension methods for generating random integers based on a <see cref="RandomRange"/> object.
    /// </summary>
    public static class RandomRangeExtension
    {
        /// <inheritdoc cref="DiceBag.Next" />
        public static int Next(this RandomRange range, IDiceBag? diceBag = null)
        {
            return (diceBag ?? DiceBag.Instance).Next(range);
        }
    }
}
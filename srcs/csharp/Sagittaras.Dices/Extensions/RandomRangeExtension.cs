using Sagittaras.Dices.Ranges;

namespace Sagittaras.Dices.Extensions
{
    public static class RandomRangeExtension
    {
        /// <inheritdoc cref="DiceBag.Next" />
        public static int Next(this RandomRange range)
        {
            return DiceBag.Next(range);
        }
    }
}
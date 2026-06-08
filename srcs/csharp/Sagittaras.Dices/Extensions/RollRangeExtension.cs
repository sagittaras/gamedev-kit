using Sagittaras.Dices.Ranges;

namespace Sagittaras.Dices.Extensions
{
    public static class RollRangeExtension
    {
        /// <inheritdoc cref="DiceBag.Roll(RollRange)" />
        public static int Roll(this RollRange range)
        {
            return DiceBag.Roll(range);
        }
    }
}
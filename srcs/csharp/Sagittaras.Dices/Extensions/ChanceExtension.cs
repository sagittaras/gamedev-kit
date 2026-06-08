using Sagittaras.Dices.Probability;

namespace Sagittaras.Dices.Extensions
{
    public static class ChanceExtension
    {
        /// <inheritdoc cref="DiceBag.TryChance(Chance)" />
        public static bool Try(this Chance chance)
        {
            return DiceBag.TryChance(chance);
        }
    }
}
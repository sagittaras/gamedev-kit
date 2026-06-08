using Sagittaras.Dices.Rolling;

namespace Sagittaras.Dices.Extensions
{
    public static class DieRollExtension
    {
        /// <inheritdoc cref="DiceBag.RollDices(DieRoll)" />
        public static int Roll(this DieRoll dieRoll)
        {
            return DiceBag.RollDices(dieRoll);
        }
    }
}
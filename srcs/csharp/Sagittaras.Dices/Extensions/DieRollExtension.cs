using Sagittaras.Dices.Rolling;

namespace Sagittaras.Dices.Extensions
{
    public static class DieRollExtension
    {
        /// <inheritdoc cref="DiceBag.Roll" />
        public static int Roll(this DieRoll dieRoll, IDiceBag diceBag)
        {
            return diceBag.Roll(dieRoll);
        }
    }
}
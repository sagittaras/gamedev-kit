using Sagittaras.Dices.Rolling;

namespace Sagittaras.Dices.Extensions
{
    /// <summary>
    ///     Provides extension methods to enhance the functionality of the <see cref="DieRoll"/> structure,
    ///     enabling simplified interaction with dice rolling mechanisms through the <see cref="IDiceBag"/> interface.
    /// </summary>
    public static class DieRollExtension
    {
        /// <inheritdoc cref="DiceBag.Roll" />
        public static int Roll(this DieRoll dieRoll, IDiceBag? diceBag = null)
        {
            return (diceBag ?? DiceBag.Instance).Roll(dieRoll);
        }
    }
}
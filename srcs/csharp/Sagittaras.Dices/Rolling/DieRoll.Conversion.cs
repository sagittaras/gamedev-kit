namespace Sagittaras.Dices.Rolling
{
    // Defines implicit conversion for DieRoll
    public readonly partial struct DieRoll
    {
        public static implicit operator DieRoll((int baseValue, int dieSides) tuple)
        {
            return new DieRoll(tuple.baseValue, tuple.dieSides);
        }
    }
}
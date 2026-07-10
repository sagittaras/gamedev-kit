namespace Sagittaras.Progression
{
    public readonly partial struct Level
    {
        public static implicit operator int(Level level)
        {
            return level.Value;
        }
    }
}
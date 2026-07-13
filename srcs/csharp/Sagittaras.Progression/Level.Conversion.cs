#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Sagittaras.Progression
{
    public readonly partial struct Level
    {
        public static Level operator +(Level a, Experience b)
        {
            return new Level(a, a.Progress + b);
        }

        public static Level operator -(Level a, Experience b)
        {
            return new Level(a, a.Progress - b);
        }
    }
}
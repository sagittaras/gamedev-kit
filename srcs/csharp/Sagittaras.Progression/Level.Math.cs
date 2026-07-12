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
using System;

namespace Sagittaras.Dices.Ranges
{
    // Defines implicit conversions for types similar to RollRange
    public readonly partial struct RollRange
    {
        public static implicit operator RollRange((int min, int max) tuple)
        {
            return new RollRange(tuple.min, tuple.max);
        }

        public static implicit operator RollRange(Range range)
        {
            return new RollRange(range.Start.Value, range.End.Value + 1);
        }
    }
}
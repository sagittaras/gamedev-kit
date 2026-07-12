#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;

namespace Sagittaras.Dices.Ranges
{
    // Defines implicit conversions for types similar to RollRange
    public readonly partial struct RandomRange
    {
        public static implicit operator RandomRange((int min, int max) tuple)
        {
            return new RandomRange(tuple.min, tuple.max);
        }

        public static implicit operator RandomRange(Range range)
        {
            return new RandomRange(range.Start.Value, range.End.Value + 1);
        }
    }
}
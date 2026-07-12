using System;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Dices.Ranges
{
    /// <summary>
    ///     Represents an inclusive roll between min and max value.
    /// </summary>
    public readonly partial struct RandomRange : IEquatable<RandomRange>, IComparable<RandomRange>
    {
        /// <summary>
        ///     Represents a roll range with a fixed single value of 1.
        /// </summary>
        public static readonly RandomRange One = new(1);

        /// <summary>
        ///     Represents a roll range spanning all possible angles in degrees.
        /// </summary>
        public static readonly RandomRange Angle = new(0, 360);
        
        /// <summary>
        ///     Creates a new <see cref="RandomRange"/> with the specified min and max values.
        /// </summary>
        /// <param name="min">Minimum inclusive value of the range.</param>
        /// <param name="max">Maximum inclusive value of the range.</param>
        public RandomRange(int min, int max)
        {
            Guard.Against.GreaterThan(min, max);
            
            Min = min;
            Max = max;
        }

        /// <summary>
        ///     Creates a new <see cref="RandomRange"/> with the specified value.
        /// </summary>
        /// <param name="value">Static value used for min and max.</param>
        public RandomRange(int value) : this(value, value)
        {
        }
        
        /// <summary>
        ///     Minimum inclusive value of the range.
        /// </summary>
        public int Min { get; }
        
        /// <summary>
        ///     Maximum inclusive value of the range.
        /// </summary>
        public int Max { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"<{Min}, {Max}>";
        }

        /// <inheritdoc />
        public bool Equals(RandomRange other)
        {
            return Min == other.Min && Max == other.Max;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is RandomRange other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Min, Max);
        }

        /// <inheritdoc />
        public int CompareTo(RandomRange other)
        {
            int minComparison = Min.CompareTo(other.Min);
            return minComparison != 0 ? minComparison : Max.CompareTo(other.Max);
        }
    }
}
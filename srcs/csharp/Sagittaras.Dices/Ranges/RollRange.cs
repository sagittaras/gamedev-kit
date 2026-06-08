using System;

namespace Sagittaras.Dices.Ranges
{
    /// <summary>
    ///     Represents an inclusive roll between min and max value.
    /// </summary>
    public readonly partial struct RollRange : IEquatable<RollRange>, IComparable<RollRange>
    {
        /// <summary>
        ///     Represents a roll range with a fixed single value of 1.
        /// </summary>
        public static readonly RollRange One = new(1);

        /// <summary>
        ///     Represents a roll range spanning all possible angles in degrees.
        /// </summary>
        public static readonly RollRange Angle = new(0, 360);
        
        public RollRange(int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentException("Min cannot be greater than max", nameof(min));
            }
            
            Min = min;
            Max = max;
        }

        public RollRange(int value) : this(value, value)
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
        public bool Equals(RollRange other)
        {
            return Min == other.Min && Max == other.Max;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is RollRange other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Min, Max);
        }

        /// <inheritdoc />
        public int CompareTo(RollRange other)
        {
            int minComparison = Min.CompareTo(other.Min);
            return minComparison != 0 ? minComparison : Max.CompareTo(other.Max);
        }
    }
}
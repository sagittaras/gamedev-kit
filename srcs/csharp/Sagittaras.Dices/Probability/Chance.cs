using System;
using Sagittaras.Dices.Ranges;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Dices.Probability
{
    /// <summary>
    ///     Representation of a chance or probability, encapsulating values from 0 to 100% in a scalable manner.
    /// </summary>
    public readonly partial struct Chance : IEquatable<Chance>, IComparable<Chance>
    {
        /// <summary>
        ///     Minimum possible value representing 0% chance.
        /// </summary>
        public const int MinValue = 0;

        /// <summary>
        ///     Maximum integer value representing 100% chance.
        /// </summary>
        public const int MaxValue = 10000;

        /// <summary>
        ///     Factor derived from the maximum value, representing 1% chance.
        /// </summary>
        public const int Factor = MaxValue / 100;

        /// <summary>
        ///     Represents the minimum possible chance value, equivalent to 0% probability.
        /// </summary>
        public static readonly Chance Min = new(MinValue);
        
        /// <summary>
        ///     Represents the maximum possible chance value, equivalent to 100% probability.
        /// </summary>
        public static readonly Chance Max = new(MaxValue);
        
        /// <summary>
        ///     Represents the half-chance value, equivalent to 50% probability.
        /// </summary>
        public static readonly Chance Half = new(MaxValue / 2);
        
        /// <summary>
        ///     Represents a range of possible chance values.
        /// </summary>
        public static readonly RandomRange Range = new(MinValue, MaxValue);

        /// <summary>
        ///     Creates a new instance of <see cref="Chance"/> from integer value.
        /// </summary>
        /// <param name="value">An integer value between <see cref="MinValue"/> and <see cref="MaxValue"/>, representing the probability.</param>
        /// <exception cref="ArgumentOutOfRangeException">The number is out of <see cref="MinValue"/> and <see cref="MaxValue"/> range.</exception>
        public Chance(int value)
        {
            Guard.Against.OutOfRange(value, MinValue, MaxValue);
            
            Value = value;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Chance"/> from floating point value.
        /// </summary>
        /// <param name="value">A probability chance as a floating point value.</param>
        public Chance(float value) : this((int)(value * Factor))
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Chance"/> from floating point value.
        /// </summary>
        /// <param name="value">A probability chance as a floating point value.</param>
        public Chance(double value) : this((int)(value * Factor))
        {
        }

        /// <summary>
        ///     Value represented as a whole integer number.
        /// </summary>
        public int Value { get; }

        /// <inheritdoc />
        public bool Equals(Chance other)
        {
            return Value == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Chance other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <inheritdoc />
        public int CompareTo(Chance other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Value / Factor}%";
        }
    }
}
using System;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Dices.Rolling
{
    /// <summary>
    ///     Represents a die roll configuration consisting of a base value and
    ///     the number of sides on the die. 
    /// </summary>
    /// <remarks>
    ///     This structure serves to model a rollable die in games or simulations, providing key properties
    ///     and operations relevant to its functionality.
    /// </remarks>
    public readonly partial struct DieRoll : IEquatable<DieRoll>, IComparable<DieRoll>
    {
        /// <summary>
        ///     Minimum number of sides a die can have.
        /// </summary>
        public const int MinDieSides = 1;

        public DieRoll(int baseValue, int dieSides = MinDieSides)
        {
            Guard.Against.GreaterThan(dieSides, MinDieSides);
            
            BaseValue = baseValue;
            DieSides = dieSides;
            MaxValue = baseValue + dieSides;
        }

        /// <summary>
        ///     Base value for the die roll calculation.
        /// </summary>
        /// <remarks>
        ///     This value represents also the minimum guaranteed outcome of the roll.
        /// </remarks>
        public int BaseValue { get; }
        
        /// <summary>
        ///     Number of sides on a die.
        /// </summary>
        /// <remarks>
        ///     This property defines the range of possible outcomes for the die roll.
        /// </remarks>
        public int DieSides { get; }
        
        /// <summary>
        ///     Maximum possible outcome of the die roll.
        /// </summary>
        public int MaxValue { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"<{BaseValue}, {MaxValue}>";
        }

        /// <inheritdoc />
        public bool Equals(DieRoll other)
        {
            return BaseValue == other.BaseValue && DieSides == other.DieSides;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is DieRoll other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(BaseValue, DieSides);
        }

        /// <inheritdoc />
        public int CompareTo(DieRoll other)
        {
            int baseValueComparison = BaseValue.CompareTo(other.BaseValue);
            return baseValueComparison != 0 ? baseValueComparison : DieSides.CompareTo(other.DieSides);
        }
    }
}
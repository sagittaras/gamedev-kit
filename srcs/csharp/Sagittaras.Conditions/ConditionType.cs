using System;

namespace Sagittaras.Conditions
{
    /// <summary>
    ///     Read-only value representing a type of condition as open enum.
    /// </summary>
    public readonly struct ConditionType : IEquatable<ConditionType>
    {
        /// <summary>
        ///     Unique numerical identifier of the condition type.
        /// </summary>
        private readonly string _id;
        
        public ConditionType(string id)
        {
            _id = id;
        }

        /// <inheritdoc />
        public bool Equals(ConditionType other)
        {
            return _id == other._id;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ConditionType other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        
        public static bool operator ==(ConditionType left, ConditionType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConditionType left, ConditionType right)
        {
            return !left.Equals(right);
        }
    }
}
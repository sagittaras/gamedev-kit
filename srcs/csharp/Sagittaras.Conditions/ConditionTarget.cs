using System;

namespace Sagittaras.Conditions
{
    public readonly struct ConditionTarget : IEquatable<ConditionTarget>
    {
        /// <summary>
        ///     Unique identifier of the target description.
        /// </summary>
        private readonly string _id;

        public ConditionTarget(string id)
        {
            _id = id;
        }

        /// <inheritdoc />
        public bool Equals(ConditionTarget other)
        {
            return _id == other._id;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ConditionTarget other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _id?.GetHashCode() ?? 0;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _id ?? string.Empty;
        }
        
        public static bool operator ==(ConditionTarget left, ConditionTarget right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConditionTarget left, ConditionTarget right)
        {
            return !left.Equals(right);
        }
    }
}
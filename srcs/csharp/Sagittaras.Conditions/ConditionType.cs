using System;

namespace Sagittaras.Conditions
{
    /// <summary>
    ///     Read-only value representing a type of condition as open enum.
    /// </summary>
    public readonly struct ConditionType : IEquatable<ConditionType>
    {
        /// <summary>
        ///     Unique identifier of the condition type.
        /// </summary>
        private readonly string _id;

        /// <summary>
        ///     Creates a condition type with the given identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the condition type.</param>
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
            return _id?.GetHashCode() ?? 0;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _id ?? string.Empty;
        }

        /// <summary>
        ///     Determines whether two condition types are equal.
        /// </summary>
        /// <param name="left">The first condition type to compare.</param>
        /// <param name="right">The second condition type to compare.</param>
        /// <returns>Returns true if both types have the same identifier; otherwise false.</returns>
        public static bool operator ==(ConditionType left, ConditionType right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Determines whether two condition types are not equal.
        /// </summary>
        /// <param name="left">The first condition type to compare.</param>
        /// <param name="right">The second condition type to compare.</param>
        /// <returns>Returns true if the types have different identifiers; otherwise false.</returns>
        public static bool operator !=(ConditionType left, ConditionType right)
        {
            return !left.Equals(right);
        }
    }
}
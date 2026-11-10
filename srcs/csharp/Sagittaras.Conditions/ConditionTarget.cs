using System;

namespace Sagittaras.Conditions
{
    /// <summary>
    ///     Read-only value representing a target of condition as open enum.
    /// </summary>
    /// <remarks>
    ///     Target selects the subject of evaluation from the evaluation context, for example the caster
    ///     or the target of a spell.
    /// </remarks>
    public readonly struct ConditionTarget : IEquatable<ConditionTarget>
    {
        /// <summary>
        ///     Unique identifier of the target description.
        /// </summary>
        private readonly string _id;

        /// <summary>
        ///     Creates a condition target with the given identifier.
        /// </summary>
        /// <param name="id">Unique identifier of the target description.</param>
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

        /// <summary>
        ///     Determines whether two condition targets are equal.
        /// </summary>
        /// <param name="left">The first condition target to compare.</param>
        /// <param name="right">The second condition target to compare.</param>
        /// <returns>Returns true if both targets have the same identifier; otherwise false.</returns>
        public static bool operator ==(ConditionTarget left, ConditionTarget right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Determines whether two condition targets are not equal.
        /// </summary>
        /// <param name="left">The first condition target to compare.</param>
        /// <param name="right">The second condition target to compare.</param>
        /// <returns>Returns true if the targets have different identifiers; otherwise false.</returns>
        public static bool operator !=(ConditionTarget left, ConditionTarget right)
        {
            return !left.Equals(right);
        }
    }
}
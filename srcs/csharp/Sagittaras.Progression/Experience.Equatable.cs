#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;

namespace Sagittaras.Progression
{
    public readonly partial struct Experience : IEquatable<Experience>
    {
        /// <inheritdoc />
        public bool Equals(Experience other)
        {
            return Value == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Experience other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value;
        }
        
        public static bool operator ==(Experience a, Experience b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(Experience a, Experience b)
        {
            return a.Value != b.Value;
        }
    }
}
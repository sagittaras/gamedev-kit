#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;

namespace Sagittaras.Progression
{
    public readonly partial struct Experience : IComparable<Experience>
    {
        /// <inheritdoc />
        public int CompareTo(Experience other)
        {
            return Value.CompareTo(other.Value);
        }
        
        public static bool operator >(Experience a, Experience b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <(Experience a, Experience b)
        {
            return a.Value < b.Value;
        }

        public static bool operator >=(Experience a, Experience b)
        {
            return a.Value >= b.Value;
        }

        public static bool operator <=(Experience a, Experience b)
        {
            return a.Value <= b.Value;
        }
    }
}
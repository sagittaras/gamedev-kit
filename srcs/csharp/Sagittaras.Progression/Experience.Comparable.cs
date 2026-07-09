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
    }
}
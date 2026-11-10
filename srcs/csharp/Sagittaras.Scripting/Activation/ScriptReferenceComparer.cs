using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sagittaras.Scripting.Activation
{
    /// <summary>
    ///     Tells the script instances apart by their reference.
    /// </summary>
    /// <remarks>
    ///     A pool reasons about the identity of the instances it handed out, which is not the same question a
    ///     script answers by overriding <see cref="object.Equals(object)"/>. Two equal scripts are still two
    ///     instances, and the default comparer would let the pool mistake them for one.
    /// </remarks>
    internal class ScriptReferenceComparer : IEqualityComparer<ITransientEntityScript>
    {
        /// <summary>
        ///     Shared instance of the comparer.
        /// </summary>
        public static readonly ScriptReferenceComparer Instance = new();

        public bool Equals(ITransientEntityScript? x, ITransientEntityScript? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(ITransientEntityScript obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
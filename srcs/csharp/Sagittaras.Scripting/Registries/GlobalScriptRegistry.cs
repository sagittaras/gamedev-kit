using System;
using System.Collections.Generic;

namespace Sagittaras.Scripting.Registries
{
    /// <inheritdoc />
    internal class GlobalScriptRegistry : IGlobalScriptRegistry
    {
        /// <summary>
        ///     Scripts registered under each entrypoint, ordered by their priority.
        /// </summary>
        /// <remarks>
        ///     Values are instances of <see cref="List{T}"/> of the entrypoint type used as the key, so a
        ///     lookup casts the whole bucket at once instead of every script in it. The generic type cannot
        ///     be expressed here, registration keeps the pairing valid.
        /// </remarks>
        private readonly Dictionary<Type, object> _scripts;

        internal GlobalScriptRegistry(Dictionary<Type, object> scripts)
        {
            _scripts = scripts;
        }

        /// <inheritdoc />
        public IReadOnlyList<TGameScript> Get<TGameScript>() where TGameScript : class, IGlobalScript
        {
            if (!_scripts.TryGetValue(typeof(TGameScript), out object? bucket))
            {
                return Array.Empty<TGameScript>();
            }

            return (IReadOnlyList<TGameScript>)bucket;
        }
    }
}
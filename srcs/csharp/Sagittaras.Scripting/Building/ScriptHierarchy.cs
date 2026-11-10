using System;
using System.Collections.Generic;
using Sagittaras.Scripting.Attributes;

namespace Sagittaras.Scripting.Building
{
    /// <inheritdoc />
    /// <remarks>
    ///     Results are cached per script type. Registration of a whole game resolves the same chains over and
    ///     over, and the walk is pure reflection.
    /// </remarks>
    public class ScriptHierarchy : IScriptHierarchy
    {
        /// <summary>
        ///     Entrypoints already resolved for a script type.
        /// </summary>
        private readonly Dictionary<Type, IReadOnlyList<Type>> _entrypoints = new();

        /// <inheritdoc />
        public IReadOnlyList<Type> GetEntrypoints(Type scriptType)
        {
            if (_entrypoints.TryGetValue(scriptType, out IReadOnlyList<Type>? cached))
            {
                return cached;
            }

            IReadOnlyList<Type> resolved = Resolve(scriptType);
            _entrypoints.Add(scriptType, resolved);

            return resolved;
        }

        /// <summary>
        ///     Walks the chain of base types and collects the entrypoints declared on it.
        /// </summary>
        /// <param name="scriptType">Type of the concrete script.</param>
        /// <returns>Returns the entrypoints ordered from the most derived one.</returns>
        private static IReadOnlyList<Type> Resolve(Type scriptType)
        {
            List<Type>? entrypoints = null;

            for (Type? current = scriptType; current is not null; current = current.BaseType)
            {
                if (!Attribute.IsDefined(current, typeof(GameScriptAttribute), false))
                {
                    continue;
                }

                entrypoints ??= new List<Type>(2);
                entrypoints.Add(current);
            }

            return entrypoints ?? (IReadOnlyList<Type>)Array.Empty<Type>();
        }
    }
}
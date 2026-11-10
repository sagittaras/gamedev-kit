using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sagittaras.Scripting.Registries
{
    /// <inheritdoc />
    internal class EntityScriptRegistry : IEntityScriptRegistry
    {
        /// <summary>
        ///     Script serving each pair of an entity and an entrypoint.
        /// </summary>
        private readonly Dictionary<EntityScriptKey, IEntityScript> _scripts;

        internal EntityScriptRegistry(Dictionary<EntityScriptKey, IEntityScript> scripts)
        {
            _scripts = scripts;
        }

        /// <inheritdoc />
        public bool TryGet<TGameScript>(string entityId, [MaybeNullWhen(false)] out TGameScript script) where TGameScript : class, IEntityScript
        {
            if (!_scripts.TryGetValue(new EntityScriptKey(entityId, typeof(TGameScript)), out IEntityScript? registered))
            {
                script = null;
                return false;
            }

            script = (TGameScript)registered;
            return true;
        }
    }
}
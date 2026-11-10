using System;

namespace Sagittaras.Scripting.Registries
{
    /// <summary>
    ///     Identifies the script serving a kind of entity under a single entrypoint.
    /// </summary>
    internal readonly struct EntityScriptKey : IEquatable<EntityScriptKey>
    {
        public EntityScriptKey(string entityId, Type entrypoint)
        {
            EntityId = entityId;
            Entrypoint = entrypoint;
        }

        /// <summary>
        ///     Identifier of the entity kind the script serves.
        /// </summary>
        public string EntityId { get; }

        /// <summary>
        ///     Entrypoint the script is reached through.
        /// </summary>
        public Type Entrypoint { get; }

        public bool Equals(EntityScriptKey other)
        {
            return Entrypoint == other.Entrypoint && string.Equals(EntityId, other.EntityId, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            return obj is EntityScriptKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(EntityId, Entrypoint);
        }

        public override string ToString()
        {
            return $"{EntityId}:{Entrypoint.Name}";
        }
    }
}
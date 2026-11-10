using System;

namespace Sagittaras.Scripting.Entities
{
    /// <summary>
    ///     Identifies a single living instance of <see cref="ITransientScriptedEntity"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <see cref="ITransientScriptedEntity.InstanceId"/> is unique only within a single
    ///         <see cref="IScriptedEntity.Id"/>, so neither part identifies the instance on its own - several
    ///         entities of different kinds may share the same instance number.
    ///     </para>
    ///     <para>
    ///         <see cref="IEquatable{T}"/> is implemented deliberately. Without it the dictionaries keyed by
    ///         this struct fall back to the reflection based comparer, which boxes the key on every single
    ///         lookup - and lookups happen on every transient script call in the game loop.
    ///     </para>
    /// </remarks>
    public readonly struct ScriptedEntityKey : IEquatable<ScriptedEntityKey>
    {
        /// <summary>
        ///     Creates the key from the parts identifying the entity instance.
        /// </summary>
        /// <param name="entityId">Identifier of the entity kind.</param>
        /// <param name="instanceId">Identifier of the instance within the entity kind.</param>
        public ScriptedEntityKey(string entityId, int instanceId)
        {
            EntityId = entityId;
            InstanceId = instanceId;
        }

        /// <summary>
        ///     Creates the key identifying the given entity instance.
        /// </summary>
        /// <param name="entity">Entity instance to create the key for.</param>
        public ScriptedEntityKey(ITransientScriptedEntity entity) : this(entity.Id, entity.InstanceId)
        {
        }

        /// <summary>
        ///     Identifier of the entity kind, as declared by <see cref="IScriptedEntity.Id"/>.
        /// </summary>
        public string EntityId { get; }

        /// <summary>
        ///     Identifier of the instance, as declared by <see cref="ITransientScriptedEntity.InstanceId"/>.
        /// </summary>
        public int InstanceId { get; }

        /// <summary>
        ///     Compares two keys for equality.
        /// </summary>
        /// <param name="left">Key on the left side of the comparison.</param>
        /// <param name="right">Key on the right side of the comparison.</param>
        /// <returns>Returns true when both keys identify the same entity instance; otherwise false.</returns>
        public static bool operator ==(ScriptedEntityKey left, ScriptedEntityKey right) => left.Equals(right);

        /// <summary>
        ///     Compares two keys for inequality.
        /// </summary>
        /// <param name="left">Key on the left side of the comparison.</param>
        /// <param name="right">Key on the right side of the comparison.</param>
        /// <returns>Returns true when the keys identify different entity instances; otherwise false.</returns>
        public static bool operator !=(ScriptedEntityKey left, ScriptedEntityKey right) => !left.Equals(right);

        /// <inheritdoc />
        public bool Equals(ScriptedEntityKey other)
        {
            return InstanceId == other.InstanceId && string.Equals(EntityId, other.EntityId, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ScriptedEntityKey other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(EntityId, InstanceId);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{EntityId}#{InstanceId}";
        }
    }
}
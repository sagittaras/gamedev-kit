using System.Diagnostics.CodeAnalysis;
using Sagittaras.Scripting.Entities;

namespace Sagittaras.Scripting.Registries
{
    /// <summary>
    ///     Holds the <see cref="IEntityScript"/> instances indexed by the entity they serve and the
    ///     entrypoints they are reachable through.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An entity holds at most one script per entrypoint - two scripts answering the same hook-point
    ///         for the same entity would be a conflict, not a broadcast. Registering a second one is refused.
    ///     </para>
    ///     <para>
    ///         The limit is per entrypoint, not per entity, so an entity may still be served by scripts from
    ///         hierarchies that never meet - being lootable and being interactive are separate contracts.
    ///     </para>
    /// </remarks>
    public interface IEntityScriptRegistry
    {
        /// <summary>
        ///     Gets the script serving the given entity under the given entrypoint.
        /// </summary>
        /// <remarks>
        ///     The instance is shared by every entity carrying the identifier. Entrypoints of transient
        ///     scripts cannot reach this method at all, as <see cref="ITransientEntityScript"/> does not
        ///     derive from <see cref="IEntityScript"/> - their instances belong to
        ///     <see cref="ITransientEntityScriptRegistry"/>.
        /// </remarks>
        /// <param name="entityId">Identifier of the entity, as declared by <see cref="IScriptedEntity.Id"/>.</param>
        /// <param name="script">Registered script, when there is one.</param>
        /// <typeparam name="TGameScript">Entrypoint to get the script for.</typeparam>
        /// <returns>Returns true when the entity is served by a script under the entrypoint; otherwise false.</returns>
        bool TryGet<TGameScript>(string entityId, [MaybeNullWhen(false)] out TGameScript script) where TGameScript : class, IEntityScript;
    }
}
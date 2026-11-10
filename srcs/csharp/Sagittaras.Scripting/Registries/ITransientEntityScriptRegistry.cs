using System;
using System.Diagnostics.CodeAnalysis;
using Sagittaras.Scripting.Entities;

namespace Sagittaras.Scripting.Registries
{
    /// <summary>
    ///     Creates, serves and discards the <see cref="ITransientEntityScript"/> instances bound to the
    ///     individual instances of a scripted entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Unlike the other two registries this one changes during the game, as entities spawn and get
    ///         released. It is not safe for concurrent use - registration, instantiation and calls are
    ///         expected to happen on the thread owning the game loop.
    ///     </para>
    ///     <para>
    ///         The registry subscribes to <see cref="ITransientScriptedEntity.Released"/> for every entity it
    ///         creates a script for, and unsubscribes when the scripts are dropped. An entity that never
    ///         raises the event keeps its scripts alive for as long as it lives.
    ///     </para>
    /// </remarks>
    public interface ITransientEntityScriptRegistry
    {
        /// <summary>
        ///     Creates the script serving the given entity instance under the given entrypoint.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The concrete script to create is the one registered for <see cref="IScriptedEntity.Id"/> of
        ///         the entity under the entrypoint. Nothing happens when the entity has no script there.
        ///     </para>
        ///     <para>
        ///         <typeparamref name="TArgs"/> is fixed by the entrypoint, so the call site cannot pass
        ///         arguments the script was not written for. Passed by <c>in</c> reference and never boxed.
        ///     </para>
        /// </remarks>
        /// <param name="entity">Entity instance to create the script for.</param>
        /// <param name="args">Arguments of the spawn, as offered by the entrypoint.</param>
        /// <typeparam name="TGameScript">Entrypoint to create the script under.</typeparam>
        /// <typeparam name="TArgs">Arguments offered by the entrypoint.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when the entity instance already holds a script reached through one of the entrypoints of this one.</exception>
        void Instantiate<TGameScript, TArgs>(ITransientScriptedEntity entity, in TArgs args) where TGameScript : class, ITransientEntityScript<TArgs>;

        /// <summary>
        ///     Creates the script serving the given entity instance under an entrypoint offering no arguments.
        /// </summary>
        /// <param name="entity">Entity instance to create the script for.</param>
        /// <typeparam name="TGameScript">Entrypoint to create the script under.</typeparam>
        void Instantiate<TGameScript>(ITransientScriptedEntity entity) where TGameScript : class, ITransientEntityScript<NoArgs>;

        /// <summary>
        ///     Gets the script serving the given entity instance under the given entrypoint.
        /// </summary>
        /// <remarks>
        ///     Yields nothing until the script is created by one of the <c>Instantiate</c> overloads, and
        ///     again once the entity has been released.
        /// </remarks>
        /// <param name="entity">Entity instance whose script is looked up.</param>
        /// <param name="script">Script serving the entity instance, when there is one.</param>
        /// <typeparam name="TGameScript">Entrypoint to get the script for.</typeparam>
        /// <returns>Returns true when the entity instance is served by a script under the entrypoint; otherwise false.</returns>
        bool TryGet<TGameScript>(ITransientScriptedEntity entity, [MaybeNullWhen(false)] out TGameScript script) where TGameScript : class, ITransientEntityScript;

        /// <summary>
        ///     Drops every script bound to the given entity instance.
        /// </summary>
        /// <remarks>
        ///     Called automatically when the entity raises <see cref="ITransientScriptedEntity.Released"/>.
        ///     Takes the key rather than the entity, so the scripts can be dropped even once the entity
        ///     itself is gone. Releasing an entity that holds no script does nothing.
        /// </remarks>
        /// <param name="key">Key of the entity instance whose scripts are dropped.</param>
        void Release(in ScriptedEntityKey key);
    }
}
using Sagittaras.Scripting.Entities;

namespace Sagittaras.Scripting
{
    /// <summary>
    ///     Counterpart of <see cref="IEntityScript"/> - while Entity Script serves all instances of
    ///     the Entity, Transient Entity Script exists per instance of the Entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Because the instance is not shared, the script may keep state of the single entity it was
    ///         created for - a combat phase, accumulated damage, a spawn timer. The instance is created by
    ///         <see cref="IScriptManager.Instantiate{TGameScript,TArgs}"/> and discarded once the entity
    ///         raises <see cref="ITransientScriptedEntity.Released"/>.
    ///     </para>
    ///     <para>
    ///         Despite the kinship the interface deliberately does not derive from
    ///         <see cref="IEntityScript"/>. The two have a different lifecycle and are served by a different
    ///         registry, and an inheritance between them would let a transient entrypoint satisfy the shared
    ///         entity calls of <see cref="IScriptManager"/> - where it would silently find nothing instead of
    ///         failing to compile.
    ///     </para>
    ///     <para>
    ///         This interface only marks the script for the storage. The script is always written against
    ///         <see cref="ITransientEntityScript{TArgs}"/>, which carries the initialization contract.
    ///     </para>
    /// </remarks>
    public interface ITransientEntityScript : IGameScript
    {
    }

    /// <summary>
    ///     Transient Entity Script initialized by the arguments its entrypoint offers.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The entrypoint - the abstract base script annotated by
    ///         <see cref="Attributes.GameScriptAttribute"/> - binds <typeparamref name="TArgs"/> once, and
    ///         every concrete script derived from it inherits that contract. An entrypoint offering nothing
    ///         but the entity itself binds <see cref="NoArgs"/>.
    ///     </para>
    ///     <para>
    ///         State is taken in <see cref="Initialize"/> rather than through a constructor, so the script can
    ///         be created by a plain <c>new</c> and, later on, rented from a pool and initialized again. The
    ///         price is that dependencies cannot be injected through the constructor nor held in readonly
    ///         fields; a container has to reach the script by property or method injection.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TArgs">
    ///     Arguments offered by the entrypoint. Declare it as a readonly struct - it travels by
    ///     <c>in</c> reference, so a struct keeps the spawn free of boxing.
    /// </typeparam>
    public interface ITransientEntityScript<TArgs> : ITransientEntityScript
    {
        /// <summary>
        ///     Binds the freshly created script to the entity instance it serves.
        /// </summary>
        /// <remarks>
        ///     Called exactly once per created instance, before any hook-point of the script is reached. An
        ///     implementation rented from a pool must overwrite all of its state here, as the instance may
        ///     carry the leftovers of the entity it served previously.
        /// </remarks>
        /// <param name="entity">Entity instance the script was created for.</param>
        /// <param name="args">Arguments of the spawn, as offered by the entrypoint.</param>
        void Initialize(ITransientScriptedEntity entity, in TArgs args);
    }
}
using System;

namespace Sagittaras.Scripting.Entities
{
    /// <summary>
    ///     Marks game entity as a target of <see cref="ITransientEntityScript"/>.
    /// </summary>
    /// <remarks>
    ///     Instance of a script is created per instance of <see cref="ITransientScriptedEntity"/>, and are not shared
    ///     between the objects of the same type.
    /// </remarks>
    public interface ITransientScriptedEntity : IScriptedEntity
    {
        /// <summary>
        ///     Unique identifier of this entity instance.
        /// </summary>
        /// <remarks>
        ///     While <see cref="IScriptedEntity.Id"/> is shared by all entities of the same kind, this value
        ///     distinguishes the individual instances between each other and pairs each of them with its own
        ///     script instance.
        /// </remarks>
        int InstanceId { get; }

        /// <summary>
        ///     Event raised when the target is released.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Transient scripting should only tie to a single lifecycle of the game entity. When advanced
        ///         techniques like pooling are used, the same object - and therefore the same
        ///         <see cref="InstanceId"/> - comes back to life representing a completely different entity,
        ///         and the state accumulated by the previous script instance would leak into it.
        ///     </para>
        ///     <para>
        ///         Event helps to clear the <see cref="ITransientEntityScript"/> instance, and allows for
        ///         the creation of a new one once the entity is instantiated again.
        ///     </para>
        /// </remarks>
        event EventHandler<ITransientScriptedEntity> Released;
    }
}
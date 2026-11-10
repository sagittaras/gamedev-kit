namespace Sagittaras.Scripting.Entities
{
    /// <summary>
    ///     Marks game entity as a target of <see cref="IEntityScript"/>.
    /// </summary>
    /// <remarks>
    ///     The entity itself carries no scripting logic - it only tells the <see cref="IScriptManager"/>
    ///     which script to look up, by matching its <see cref="Id"/> against the identifier declared by
    ///     <see cref="Attributes.ScriptedEntityAttribute"/>.
    /// </remarks>
    public interface IScriptedEntity
    {
        /// <summary>
        ///     Unique identifier of the entity.
        /// </summary>
        /// <remarks>
        ///     Identifies the entity type, not its instance - typically a template or database identifier
        ///     shared by all entities of the same kind.
        /// </remarks>
        string Id { get; }
    }
}
namespace Sagittaras.Scripting
{
    /// <summary>
    ///     Entity script represents extensible hook-points for further customization of the behavior of a specific game entity.
    /// </summary>
    /// <remarks>
    ///     The script is bound to an entity by its identifier - the implementation declares the target through
    ///     <see cref="Attributes.ScriptedEntityAttribute"/> and the core reaches it by passing an
    ///     <see cref="Entities.IScriptedEntity"/> to <see cref="IScriptManager"/>. A single script instance is
    ///     shared by every entity carrying that identifier, so it must stay stateless with respect to the
    ///     individual entity - per-instance state belongs to <see cref="ITransientEntityScript"/>.
    /// </remarks>
    public interface IEntityScript : IGameScript
    {
    }
}
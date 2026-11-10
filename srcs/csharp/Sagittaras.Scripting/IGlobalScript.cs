namespace Sagittaras.Scripting
{
    /// <summary>
    ///     Global Script represents extensible hook-points without a specific game entity target.
    /// </summary>
    /// <remarks>
    ///     Use it for hook-points that belong to a system rather than to a single entity - loot rolls,
    ///     difficulty scaling, save migrations and similar cross-cutting behavior. Every registered
    ///     implementation of the same base script is a valid target of a call, so a single hook-point may be
    ///     answered by several extending modules at once.
    /// </remarks>
    public interface IGlobalScript : IGameScript
    {
    }
}
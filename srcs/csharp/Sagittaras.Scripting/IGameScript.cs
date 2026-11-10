namespace Sagittaras.Scripting
{
    /// <summary>
    ///     General marker interface for scripting extensions.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Game Script allows for wiring custom logic into the core library of your game by providing
    ///         extensible hook-points for further customization of the behavior.
    ///     </para>
    ///     <para>
    ///         The core module declares an abstract base script annotated by
    ///         <see cref="Attributes.GameScriptAttribute"/>, whose members are the hook-points it is willing
    ///         to expose. An extending module derives from that base, overrides only the hook-points it cares
    ///         about, and the core calls them back through <see cref="IScriptManager"/> without ever
    ///         referencing the extension.
    ///     </para>
    ///     <para>
    ///         This interface is never implemented directly - derive from <see cref="IGlobalScript"/>,
    ///         <see cref="IEntityScript"/> or <see cref="ITransientEntityScript"/>, which differ in what the
    ///         script is bound to and how long its instance lives.
    ///     </para>
    /// </remarks>
    public interface IGameScript
    {
    }
}
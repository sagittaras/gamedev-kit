using System;

namespace Sagittaras.Scripting.Attributes
{
    /// <summary>
    ///     Marks abstract class as an available and registrable entrypoint.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The marked class is the contract declared by the core module: its virtual members are the
    ///         hook-points, and every concrete script derived from it is discovered and registered under it.
    ///         Calls made through <see cref="IScriptManager"/> are resolved against this base type, which is
    ///         what keeps the core free of any reference to the extending modules.
    ///     </para>
    ///     <para>
    ///         Name the hook-points after the moment they open up - <c>OnAggro</c>, <c>OnDamageTaken</c>. Read
    ///         top to bottom, the marked class is then the list of moments an extending module can step into.
    ///         A hook-point returning a value asks a question at that moment instead of announcing it: the
    ///         core hands in what it would have used on its own, and the script answers or passes it back.
    ///     </para>
    ///     <para>
    ///         Classes without this attribute will not be registered as entrypoint.
    ///     </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class GameScriptAttribute : Attribute
    {
    }
}
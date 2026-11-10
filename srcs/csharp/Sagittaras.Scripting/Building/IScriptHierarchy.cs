using System;
using System.Collections.Generic;
using Sagittaras.Scripting.Attributes;

namespace Sagittaras.Scripting.Building
{
    /// <summary>
    ///     Resolves the entrypoints a concrete script can be reached through.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Scripts form a hierarchy of entrypoints - an object script, a world object script derived from
    ///         it, a creature script derived from that - and the core module calls a hook-point at whichever
    ///         level it is declared on. A concrete script therefore has to be reachable by every entrypoint
    ///         above it, not only by the one it derives from directly.
    ///     </para>
    ///     <para>
    ///         The registries resolve this once during registration and index the script under each of its
    ///         entrypoints. A call is then a single lookup by the requested type, with no assignability
    ///         testing left for the game loop.
    ///     </para>
    /// </remarks>
    public interface IScriptHierarchy
    {
        /// <summary>
        ///     Gets the entrypoints the given script is registered under.
        /// </summary>
        /// <remarks>
        ///     Walks the chain of base types and collects those annotated by <see cref="GameScriptAttribute"/>,
        ///     including the script itself when it carries the attribute. Because the attribute is not
        ///     inherited, every level of the hierarchy has to opt in on its own.
        /// </remarks>
        /// <param name="scriptType">Type of the concrete script.</param>
        /// <returns>
        ///     Returns the entrypoints ordered from the most derived one, or an empty list when the script
        ///     derives from no entrypoint at all.
        /// </returns>
        IReadOnlyList<Type> GetEntrypoints(Type scriptType);
    }
}
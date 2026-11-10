using System.Collections.Generic;
using Sagittaras.Scripting.Attributes;

namespace Sagittaras.Scripting.Registries
{
    /// <summary>
    ///     Holds the <see cref="IGlobalScript"/> instances indexed by the entrypoints they are reachable through.
    /// </summary>
    /// <remarks>
    ///     Global hook-points are a broadcast - several modules may extend the same one, so the registry
    ///     keeps a collection per entrypoint rather than a single script.
    /// </remarks>
    public interface IGlobalScriptRegistry
    {
        /// <summary>
        ///     Gets every script registered under the given entrypoint.
        /// </summary>
        /// <remarks>
        ///     Scripts come ordered by <see cref="ScriptPriorityAttribute"/>, the highest priority first. An
        ///     entrypoint nobody extends yields an empty list - it is an ordinary state of a hook-point, not
        ///     an error, and costs no allocation.
        /// </remarks>
        /// <typeparam name="TGameScript">Entrypoint to get the scripts for.</typeparam>
        /// <returns>Returns the registered scripts, or an empty list when there is none.</returns>
        IReadOnlyList<TGameScript> Get<TGameScript>() where TGameScript : class, IGlobalScript;
    }
}
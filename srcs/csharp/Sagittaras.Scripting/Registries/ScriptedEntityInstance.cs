using System;
using System.Collections.Generic;
using Sagittaras.Scripting.Entities;

namespace Sagittaras.Scripting.Registries
{
    /// <summary>
    ///     Scripts currently bound to a single living instance of a scripted entity.
    /// </summary>
    /// <remarks>
    ///     Holds the entity itself, not only its key - the registry has to reach it again to unsubscribe from
    ///     <see cref="ITransientScriptedEntity.Released"/> once the scripts are dropped.
    /// </remarks>
    internal class ScriptedEntityInstance
    {
        internal ScriptedEntityInstance(ITransientScriptedEntity entity)
        {
            Entity = entity;
        }

        /// <summary>
        ///     Entity instance the scripts belong to.
        /// </summary>
        public ITransientScriptedEntity Entity { get; }

        /// <summary>
        ///     Scripts indexed by every entrypoint they are reachable through.
        /// </summary>
        /// <remarks>
        ///     A single script appears under each of its entrypoints, so the same instance is present
        ///     several times over.
        /// </remarks>
        public Dictionary<Type, ITransientEntityScript> Scripts { get; } = new();
    }
}
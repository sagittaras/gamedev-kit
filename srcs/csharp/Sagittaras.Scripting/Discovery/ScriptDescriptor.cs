using System;
using System.Collections.Generic;

namespace Sagittaras.Scripting.Discovery
{
    /// <summary>
    ///     Describes a script that was found and the way it should be registered.
    /// </summary>
    /// <remarks>
    ///     The descriptor is the seam between finding the scripts and registering them. Reflection over the
    ///     assemblies is only one way to arrive at these - a source generator producing them ahead of time
    ///     registers exactly the same way, which matters on AOT builds with stripped code.
    /// </remarks>
    public readonly struct ScriptDescriptor
    {
        private ScriptDescriptor(Type scriptType, ScriptKind kind, IReadOnlyList<string> entityIds)
        {
            ScriptType = scriptType;
            Kind = kind;
            EntityIds = entityIds;
        }

        /// <summary>
        ///     Concrete type of the discovered script.
        /// </summary>
        public Type ScriptType { get; }

        /// <summary>
        ///     Registry the script belongs to.
        /// </summary>
        public ScriptKind Kind { get; }

        /// <summary>
        ///     Entities the script is written for, empty for a global script.
        /// </summary>
        public IReadOnlyList<string> EntityIds { get; }

        /// <summary>
        ///     Describes a script extending the hook-points without an entity target.
        /// </summary>
        /// <param name="scriptType">Concrete type of the script.</param>
        /// <returns>Returns the descriptor of the script.</returns>
        public static ScriptDescriptor Global(Type scriptType)
        {
            return new ScriptDescriptor(scriptType, ScriptKind.Global, Array.Empty<string>());
        }

        /// <summary>
        ///     Describes a script shared by all instances of the given entities.
        /// </summary>
        /// <param name="scriptType">Concrete type of the script.</param>
        /// <param name="entityIds">Entities the script is written for.</param>
        /// <returns>Returns the descriptor of the script.</returns>
        public static ScriptDescriptor Entity(Type scriptType, IReadOnlyList<string> entityIds)
        {
            return new ScriptDescriptor(scriptType, ScriptKind.Entity, entityIds);
        }

        /// <summary>
        ///     Describes a script created per instance of the given entities.
        /// </summary>
        /// <param name="scriptType">Concrete type of the script.</param>
        /// <param name="entityIds">Entities the script is written for.</param>
        /// <returns>Returns the descriptor of the script.</returns>
        public static ScriptDescriptor Transient(Type scriptType, IReadOnlyList<string> entityIds)
        {
            return new ScriptDescriptor(scriptType, ScriptKind.Transient, entityIds);
        }
    }
}
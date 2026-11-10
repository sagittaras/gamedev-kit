using System;
using Sagittaras.Scripting.Entities;

namespace Sagittaras.Scripting.Attributes
{
    /// <summary>
    ///     Identifies the target entity for instances of <see cref="IScriptedEntity"/> or <see cref="ITransientScriptedEntity"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Required to automatically register the script inside <see cref="IScriptManager"/>.
    ///     </para>
    ///     <para>
    ///         Not inherited - a script deriving from another one would otherwise take over its target and
    ///         collide with it. Every script names its own entity, the same way every level of the hierarchy
    ///         declares itself an entrypoint by <see cref="GameScriptAttribute"/>.
    ///     </para>
    ///     <para>
    ///         Repeat the attribute to let a single script serve several entities. A shared entity script is
    ///         then one instance answering for all of them, while a transient one is created per instance of
    ///         each of them.
    ///     </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ScriptedEntityAttribute : Attribute
    {
        /// <summary>
        ///     Declares the entity the annotated script is written for.
        /// </summary>
        /// <param name="id">
        ///     Identifier of the target entity, matched against <see cref="IScriptedEntity.Id"/> when the
        ///     script is being looked up.
        /// </param>
        public ScriptedEntityAttribute(string id)
        {
            Id = id;
        }

        /// <summary>
        ///     Identifier of the entity the annotated script belongs to.
        /// </summary>
        public string Id { get; }
    }
}
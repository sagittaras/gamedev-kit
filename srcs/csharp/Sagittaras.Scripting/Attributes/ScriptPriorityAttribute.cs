using System;

namespace Sagittaras.Scripting.Attributes
{
    /// <summary>
    ///     Declares the order in which the script answers a hook-point shared with other scripts.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Applies to <see cref="IGlobalScript"/> only. Entity and transient hook-points hold at most one
    ///         script per entrypoint, so there is nothing to order - the attribute is ignored there.
    ///     </para>
    ///     <para>
    ///         Without the attribute the script takes <see cref="DefaultPriority"/> and ends up ordered by
    ///         the moment of its registration, which depends on the order the assemblies were loaded in.
    ///         Declare the priority explicitly wherever the outcome of a hook-point must not depend on it.
    ///     </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ScriptPriorityAttribute : Attribute
    {
        /// <summary>
        ///     Priority taken by a script that does not declare the attribute.
        /// </summary>
        public const int DefaultPriority = 0;

        /// <summary>
        ///     Declares the priority of the annotated script.
        /// </summary>
        /// <param name="priority">Priority of the script, where a higher value is called earlier.</param>
        public ScriptPriorityAttribute(int priority)
        {
            Priority = priority;
        }

        /// <summary>
        ///     Priority of the script, where a higher value is called earlier.
        /// </summary>
        public int Priority { get; }
    }
}
namespace Sagittaras.Scripting.Discovery
{
    /// <summary>
    ///     Tells which registry a discovered script belongs to.
    /// </summary>
    public enum ScriptKind
    {
        /// <summary>
        ///     Script extending the hook-points without an entity target.
        /// </summary>
        Global,

        /// <summary>
        ///     Script shared by all instances of the entities it is written for.
        /// </summary>
        Entity,

        /// <summary>
        ///     Script created per instance of the entities it is written for.
        /// </summary>
        Transient
    }
}
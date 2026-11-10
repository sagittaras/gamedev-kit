namespace Sagittaras.Scripting.Building
{
    /// <summary>
    ///     Global script waiting to be ordered into the bucket of an entrypoint.
    /// </summary>
    internal readonly struct GlobalScriptRegistration
    {
        public GlobalScriptRegistration(IGlobalScript script, int priority, int order)
        {
            Script = script;
            Priority = priority;
            Order = order;
        }

        /// <summary>
        ///     Registered instance of the script.
        /// </summary>
        public IGlobalScript Script { get; }

        /// <summary>
        ///     Declared priority of the script, where a higher value is called earlier.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        ///     Order the script was registered in.
        /// </summary>
        /// <remarks>
        ///     Breaks the ties of equal priorities, so that the outcome does not depend on the sorting
        ///     algorithm being stable.
        /// </remarks>
        public int Order { get; }
    }
}
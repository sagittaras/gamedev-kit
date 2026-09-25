namespace Sagittaras.DevelopmentKit.Editor
{
    /// <summary>
    ///     How a <see cref="PreImageBuild"/> ended.
    /// </summary>
    internal enum PreImageBuildStatus
    {
        /// <summary>
        ///     The build succeeded; the pre-images of the built packages are refreshed or were already up to date.
        /// </summary>
        Succeeded,

        /// <summary>
        ///     The build failed, or could not even start.
        /// </summary>
        Failed,

        /// <summary>
        ///     The build was cancelled from Unity's Background Tasks before it finished.
        /// </summary>
        Cancelled,
    }
}

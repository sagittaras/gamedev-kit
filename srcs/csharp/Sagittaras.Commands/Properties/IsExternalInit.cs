namespace System.Runtime.CompilerServices
{
    /// <summary>
    ///     Lets the compiler emit init accessors for a target that does not ship the type.
    /// </summary>
    /// <remarks>
    ///     C# 9 marks an init accessor with this type, which only arrived in .NET 5. The package targets
    ///     netstandard2.1, so it declares the type itself - internally, so no other assembly ever sees it or
    ///     collides with a declaration of its own.
    /// </remarks>
    internal static class IsExternalInit
    {
    }
}

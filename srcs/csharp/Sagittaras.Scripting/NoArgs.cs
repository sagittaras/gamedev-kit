namespace Sagittaras.Scripting
{
    /// <summary>
    ///     Arguments of a script that needs nothing but the entity it was created for.
    /// </summary>
    /// <remarks>
    ///     Keeps a single initialization contract for every transient script - an entrypoint offering no
    ///     spawn arguments declares <see cref="ITransientEntityScript{TArgs}"/> over this empty struct
    ///     instead of a second, argument-less interface.
    /// </remarks>
    public readonly struct NoArgs
    {
    }
}
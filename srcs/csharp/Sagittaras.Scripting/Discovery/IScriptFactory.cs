using System;

namespace Sagittaras.Scripting.Discovery
{
    /// <summary>
    ///     Creates the single instance of a global or a shared entity script.
    /// </summary>
    /// <remarks>
    ///     These scripts are created once while the game boots, so unlike the transient ones they are free to
    ///     take their dependencies through the constructor. Replacing the factory is what lets a container
    ///     resolve them.
    /// </remarks>
    public interface IScriptFactory
    {
        /// <summary>
        ///     Creates the instance of the discovered script.
        /// </summary>
        /// <param name="scriptType">Concrete type of the script.</param>
        /// <returns>Returns the created instance.</returns>
        IGameScript Create(Type scriptType);
    }
}
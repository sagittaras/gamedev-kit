using System;

namespace Sagittaras.Scripting.Discovery
{
    /// <inheritdoc />
    /// <remarks>
    ///     Creates the script by its parameterless constructor.
    /// </remarks>
    public class ActivatorScriptFactory : IScriptFactory
    {
        /// <inheritdoc />
        /// <exception cref="ArgumentException">Thrown when the script cannot be created by a parameterless constructor.</exception>
        public IGameScript Create(Type scriptType)
        {
            try
            {
                return (IGameScript)Activator.CreateInstance(scriptType)!;
            }
            catch (MissingMethodException exception)
            {
                throw new ArgumentException($"Script `{scriptType}` cannot be created, as it has no public parameterless constructor. Register it by hand or replace the `{nameof(IScriptFactory)}`.", nameof(scriptType), exception);
            }
        }
    }
}
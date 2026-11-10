using System;
using Sagittaras.Scripting.Activation;
using System.Collections.Generic;

namespace Sagittaras.Scripting.Registries
{
    /// <summary>
    ///     Describes how a transient script is created and where it is indexed once it exists.
    /// </summary>
    /// <remarks>
    ///     Both parts are resolved while the concrete type is still known statically, so a spawn needs no
    ///     reflection at all.
    /// </remarks>
    internal readonly struct TransientScriptRegistration
    {
        public TransientScriptRegistration(Func<IScriptActivator, ITransientEntityScript> factory, IReadOnlyList<Type> entrypoints)
        {
            Factory = factory;
            Entrypoints = entrypoints;
        }

        /// <summary>
        ///     Takes an instance of the concrete script from the activator.
        /// </summary>
        public Func<IScriptActivator, ITransientEntityScript> Factory { get; }

        /// <summary>
        ///     Entrypoints the created instance is indexed under.
        /// </summary>
        public IReadOnlyList<Type> Entrypoints { get; }
    }
}
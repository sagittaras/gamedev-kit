namespace Sagittaras.Scripting.Activation
{
    /// <summary>
    ///     Provides and takes back the instances of transient scripts.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Instances come from a plain <c>new</c>, which is why the transient scripts are constrained to a
    ///         parameterless constructor - it keeps the spawn free of reflection and safe on AOT runtimes.
    ///         State reaches the script through <see cref="ITransientEntityScript{TArgs}.Initialize"/>
    ///         afterwards, which is also what lets a released instance serve the next entity.
    ///     </para>
    ///     <para>
    ///         <see cref="PoolingScriptActivator"/> serves by default. Replacing it is the seam for a
    ///         container that injects the dependencies into the created script, or for a pool of your own.
    ///     </para>
    /// </remarks>
    public interface IScriptActivator
    {
        /// <summary>
        ///     Provides an instance of the script to be initialized.
        /// </summary>
        /// <remarks>
        ///     A pooled instance may carry the state of the entity it served before - the caller initializes
        ///     it before it reaches any hook-point.
        /// </remarks>
        /// <typeparam name="TScript">Concrete type of the script to provide.</typeparam>
        /// <returns>Returns the instance to be initialized.</returns>
        TScript Create<TScript>() where TScript : class, ITransientEntityScript, new();

        /// <summary>
        ///     Takes back a script that is no longer bound to any entity instance.
        /// </summary>
        /// <param name="script">Script dropped by the registry.</param>
        void Release(ITransientEntityScript script);
    }
}
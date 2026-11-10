using System.Reflection;
using Sagittaras.Scripting.Building;

namespace Sagittaras.Scripting.Discovery
{
    /// <summary>
    ///     Shorthands for registering the scripts a module holds.
    /// </summary>
    public static class ScriptDiscoveryExtensions
    {
        /// <summary>
        ///     Finds every script of the assembly and registers it.
        /// </summary>
        /// <param name="builder">Builder collecting the scripts of all modules.</param>
        /// <param name="assembly">Assembly of a module holding the scripts.</param>
        /// <returns>Returns the builder for chaining.</returns>
        public static IScriptRegistryBuilder Discover(this IScriptRegistryBuilder builder, Assembly assembly)
        {
            return builder.Discover(assembly, new ScriptDiscovery());
        }

        /// <summary>
        ///     Finds every script of the assembly and registers it through the given discovery.
        /// </summary>
        /// <param name="builder">Builder collecting the scripts of all modules.</param>
        /// <param name="assembly">Assembly of a module holding the scripts.</param>
        /// <param name="discovery">Discovery finding and registering the scripts.</param>
        /// <returns>Returns the builder for chaining.</returns>
        public static IScriptRegistryBuilder Discover(this IScriptRegistryBuilder builder, Assembly assembly, ScriptDiscovery discovery)
        {
            return discovery.Register(builder, discovery.Scan(assembly));
        }
    }
}
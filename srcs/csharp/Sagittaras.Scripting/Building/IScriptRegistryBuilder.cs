using System;
using Sagittaras.Scripting.Activation;
using Sagittaras.Scripting.Entities;

namespace Sagittaras.Scripting.Building
{
    /// <summary>
    ///     Collects the scripts of all modules and builds the <see cref="IScriptManager"/> serving them.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Registration is deliberately the expensive half of the system. Resolving the entrypoints of a
    ///         script, ordering by priority and capturing how a transient script is created all happen here,
    ///         once during the boot, so that a hook-point reached from the game loop is a plain lookup.
    ///     </para>
    ///     <para>
    ///         Discovering the scripts - scanning the assemblies for
    ///         <see cref="Attributes.ScriptedEntityAttribute"/> and the entrypoints - is not a part of the
    ///         builder. It is a separate concern feeding these methods, so that an environment where
    ///         reflection is unreliable, such as an AOT build with stripped code, can replace it without
    ///         touching the registries.
    ///     </para>
    /// </remarks>
    public interface IScriptRegistryBuilder
    {
        /// <summary>
        ///     Registers a script extending the hook-points that have no entity target.
        /// </summary>
        /// <remarks>
        ///     The instance is created by the caller and lives for as long as the built manager does.
        /// </remarks>
        /// <param name="script">Instance of the script to register.</param>
        /// <returns>Returns the builder for chaining.</returns>
        IScriptRegistryBuilder AddGlobal(IGlobalScript script);

        /// <summary>
        ///     Registers a script extending the hook-points of every entity carrying the identifier.
        /// </summary>
        /// <remarks>
        ///     The instance is shared by all of those entities, so it must not keep state of any single one.
        /// </remarks>
        /// <param name="entityId">Identifier of the entity, as declared by <see cref="IScriptedEntity.Id"/>.</param>
        /// <param name="script">Instance of the script to register.</param>
        /// <returns>Returns the builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the entity already has a script under any of the entrypoints of this one.</exception>
        IScriptRegistryBuilder AddEntity(string entityId, IEntityScript script);

        /// <summary>
        ///     Registers a script to be created per instance of every entity carrying the identifier.
        /// </summary>
        /// <remarks>
        ///     Takes the type rather than an instance - the instances are created later, on the spawns of the
        ///     entity. The parameterless constructor is required here and nowhere else: capturing it while
        ///     the concrete type is still known statically is what keeps the spawn itself free of reflection.
        /// </remarks>
        /// <param name="entityId">Identifier of the entity, as declared by <see cref="IScriptedEntity.Id"/>.</param>
        /// <typeparam name="TScript">Concrete type of the script to register.</typeparam>
        /// <returns>Returns the builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the entity already has a script under any of the entrypoints of this one.</exception>
        IScriptRegistryBuilder AddTransient<TScript>(string entityId) where TScript : class, ITransientEntityScript, new();

        /// <summary>
        ///     Replaces the way the transient script instances are created.
        /// </summary>
        /// <param name="activator">Activator providing the instances.</param>
        /// <returns>Returns the builder for chaining.</returns>
        IScriptRegistryBuilder WithActivator(IScriptActivator activator);

        /// <summary>
        ///     Creates the manager from the current registrations.
        /// </summary>
        /// <remarks>
        ///     Registrations are copied, so further changes of the builder do not affect already created managers.
        /// </remarks>
        /// <returns>Returns the manager serving the registered scripts.</returns>
        IScriptManager Build();
    }
}
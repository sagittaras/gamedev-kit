using System;
using Sagittaras.Scripting.Building;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sagittaras.Scripting.Attributes;

namespace Sagittaras.Scripting.Discovery
{
    /// <summary>
    ///     Finds the scripts of the loaded modules and registers them into an <see cref="IScriptRegistryBuilder"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Kept apart from the registries on purpose. Scanning is reflection over the assemblies, which is
    ///         exactly what an AOT build with stripped code cannot be trusted with - swapping it for a source
    ///         generator producing <see cref="ScriptDescriptor"/> ahead of time leaves the registration and
    ///         the registries untouched.
    ///     </para>
    ///     <para>
    ///         A type implementing <see cref="IGameScript"/> that derives from no entrypoint is skipped rather
    ///         than reported. It is not a part of the system, as nothing could ever call it. Mind the flip
    ///         side: forgetting <see cref="GameScriptAttribute"/> on a base script silently leaves every
    ///         script below it unregistered.
    ///     </para>
    /// </remarks>
    public class ScriptDiscovery
    {
        /// <summary>
        ///     Generic definition of the registration of a transient script.
        /// </summary>
        /// <remarks>
        ///     Transient scripts are registered by their concrete type, which the builder needs statically to
        ///     capture their constructor. Discovery only has the type at hand, so it reaches the method
        ///     through reflection - once per script while booting, never on a spawn.
        /// </remarks>
        private static readonly MethodInfo AddTransientDefinition = typeof(IScriptRegistryBuilder)
            .GetMethod(nameof(IScriptRegistryBuilder.AddTransient))!;

        /// <summary>
        ///     Resolves the entrypoints of the scanned types.
        /// </summary>
        private readonly IScriptHierarchy _hierarchy;

        /// <summary>
        ///     Creates the instances of global and shared entity scripts.
        /// </summary>
        private readonly IScriptFactory _factory;

        /// <summary>
        ///     Creates the discovery over the default factory of script instances.
        /// </summary>
        public ScriptDiscovery() : this(new ActivatorScriptFactory(), new ScriptHierarchy())
        {
        }

        /// <summary>
        ///     Creates the discovery over the given factory of script instances.
        /// </summary>
        /// <param name="factory">Creates the instances of global and shared entity scripts.</param>
        public ScriptDiscovery(IScriptFactory factory) : this(factory, new ScriptHierarchy())
        {
        }

        /// <summary>
        ///     Creates the discovery over the given factory and resolver of entrypoints.
        /// </summary>
        /// <param name="factory">Creates the instances of global and shared entity scripts.</param>
        /// <param name="hierarchy">Resolves the entrypoints of the scanned types.</param>
        public ScriptDiscovery(IScriptFactory factory, IScriptHierarchy hierarchy)
        {
            _factory = factory;
            _hierarchy = hierarchy;
        }

        /// <summary>
        ///     Finds every script the assembly holds.
        /// </summary>
        /// <remarks>
        ///     Types that failed to load are skipped, as a single missing optional dependency would otherwise
        ///     take the whole scan down with it.
        /// </remarks>
        /// <param name="assembly">Assembly of a module holding the scripts.</param>
        /// <returns>Returns the descriptor of every script found.</returns>
        public IEnumerable<ScriptDescriptor> Scan(Assembly assembly)
        {
            return GetLoadableTypes(assembly).Where(IsScript).Select(Describe);
        }

        /// <summary>
        ///     Registers the described scripts into the builder.
        /// </summary>
        /// <param name="builder">Builder collecting the scripts of all modules.</param>
        /// <param name="descriptors">Scripts to register.</param>
        /// <returns>Returns the builder for chaining.</returns>
        public IScriptRegistryBuilder Register(IScriptRegistryBuilder builder, IEnumerable<ScriptDescriptor> descriptors)
        {
            foreach (ScriptDescriptor descriptor in descriptors)
            {
                Register(builder, descriptor);
            }

            return builder;
        }

        /// <summary>
        ///     Registers a single described script into the builder.
        /// </summary>
        /// <param name="builder">Builder collecting the scripts of all modules.</param>
        /// <param name="descriptor">Script to register.</param>
        /// <returns>Returns the builder for chaining.</returns>
        public IScriptRegistryBuilder Register(IScriptRegistryBuilder builder, in ScriptDescriptor descriptor)
        {
            switch (descriptor.Kind)
            {
                case ScriptKind.Global:
                    return builder.AddGlobal((IGlobalScript)_factory.Create(descriptor.ScriptType));

                case ScriptKind.Entity:
                    IEntityScript script = (IEntityScript)_factory.Create(descriptor.ScriptType);
                    for (int i = 0; i < descriptor.EntityIds.Count; i++)
                    {
                        builder.AddEntity(descriptor.EntityIds[i], script);
                    }

                    return builder;

                case ScriptKind.Transient:
                    MethodInfo add = MakeAddTransient(descriptor.ScriptType);
                    for (int i = 0; i < descriptor.EntityIds.Count; i++)
                    {
                        InvokeAddTransient(add, builder, descriptor.EntityIds[i]);
                    }

                    return builder;

                default:
                    throw new ArgumentOutOfRangeException(nameof(descriptor), descriptor.Kind, "Unknown kind of a script.");
            }
        }

        /// <summary>
        ///     Tells whether the type is a script that can be registered.
        /// </summary>
        /// <param name="type">Type found in the scanned assembly.</param>
        /// <returns>Returns true when the type is a concrete script deriving from an entrypoint.</returns>
        private bool IsScript(Type type)
        {
            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
            {
                return false;
            }

            return typeof(IGameScript).IsAssignableFrom(type) && _hierarchy.GetEntrypoints(type).Count > 0;
        }

        /// <summary>
        ///     Describes how a script should be registered, the way <see cref="Scan"/> would.
        /// </summary>
        /// <remarks>
        ///     Useful on its own where the scripts are listed by hand instead of scanned, as an AOT build
        ///     with stripped code may have to do.
        /// </remarks>
        /// <param name="type">Concrete type of the script.</param>
        /// <returns>Returns the descriptor of the script.</returns>
        /// <exception cref="ArgumentException">Thrown when the script targets several registries at once, or when it names no entity while needing one.</exception>
        public static ScriptDescriptor Describe(Type type)
        {
            bool isTransient = typeof(ITransientEntityScript).IsAssignableFrom(type);
            bool isEntity = typeof(IEntityScript).IsAssignableFrom(type);
            bool isGlobal = typeof(IGlobalScript).IsAssignableFrom(type);

            if (isTransient ? isEntity || isGlobal : isEntity && isGlobal)
            {
                throw new ArgumentException($"Script `{type}` is a global, a shared and a transient script at once, so there is no telling which registry should hold it.", nameof(type));
            }

            if (isGlobal)
            {
                RequireNoEntities(type);
                return ScriptDescriptor.Global(type);
            }

            IReadOnlyList<string> entityIds = RequireEntities(type);
            return isTransient ? ScriptDescriptor.Transient(type, entityIds) : ScriptDescriptor.Entity(type, entityIds);
        }

        /// <summary>
        ///     Reads the entities the script is written for.
        /// </summary>
        /// <remarks>
        ///     Attributes of the base types are not taken - a script deriving from another one would inherit
        ///     its target and collide with it. Every script names its own entities, the same way every level
        ///     of the hierarchy declares itself an entrypoint.
        /// </remarks>
        /// <param name="type">Concrete type of the script.</param>
        /// <returns>Returns the entities named by the script.</returns>
        /// <exception cref="ArgumentException">Thrown when the script names no entity.</exception>
        private static IReadOnlyList<string> RequireEntities(Type type)
        {
            List<string> entityIds = type.GetCustomAttributes<ScriptedEntityAttribute>(false)
                .Select(attribute => attribute.Id)
                .ToList();

            return entityIds.Count == 0 
                ? throw new ArgumentException($"Script `{type}` serves an entity, but names none by `{nameof(ScriptedEntityAttribute)}`, so nothing could ever reach it.", nameof(type)) 
                : entityIds;
        }

        /// <summary>
        ///     Verifies that a global script names no entity.
        /// </summary>
        /// <param name="type">Concrete type of the script.</param>
        /// <exception cref="ArgumentException">Thrown when a global script names an entity it cannot serve.</exception>
        private static void RequireNoEntities(Type type)
        {
            if (!type.GetCustomAttributes<ScriptedEntityAttribute>(false).Any())
            {
                return;
            }

            throw new ArgumentException($"Script `{type}` names an entity by `{nameof(ScriptedEntityAttribute)}`, but extends a hook-point without an entity target.", nameof(type));
        }

        /// <summary>
        ///     Binds the registration of a transient script to its concrete type.
        /// </summary>
        /// <param name="type">Concrete type of the script.</param>
        /// <returns>Returns the bound registration method.</returns>
        /// <exception cref="ArgumentException">Thrown when the script has no public parameterless constructor.</exception>
        private static MethodInfo MakeAddTransient(Type type)
        {
            try
            {
                return AddTransientDefinition.MakeGenericMethod(type);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException($"Transient script `{type}` cannot be registered, as it has no public parameterless constructor. Its state belongs to `{nameof(ITransientEntityScript<object>.Initialize)}`.", nameof(type), exception);
            }
        }

        /// <summary>
        ///     Registers the transient script for a single entity.
        /// </summary>
        /// <remarks>
        ///     Unwraps the reflection wrapper, so that a collision of two scripts reaches the caller as the
        ///     exception the builder threw rather than as a <see cref="TargetInvocationException"/>.
        /// </remarks>
        /// <param name="add">Registration method bound to the concrete type.</param>
        /// <param name="builder">Builder collecting the scripts of all modules.</param>
        /// <param name="entityId">Entity the script is registered for.</param>
        private static void InvokeAddTransient(MethodInfo add, IScriptRegistryBuilder builder, string entityId)
        {
            try
            {
                add.Invoke(builder, new object[] { entityId });
            }
            catch (TargetInvocationException exception) when (exception.InnerException is not null)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        ///     Gets the types of the assembly, leaving out those that failed to load.
        /// </summary>
        /// <param name="assembly">Assembly of a module holding the scripts.</param>
        /// <returns>Returns the types that could be loaded.</returns>
        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type is not null)!;
            }
        }
    }
}
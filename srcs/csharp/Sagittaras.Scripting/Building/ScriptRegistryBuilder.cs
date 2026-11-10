using System;
using Sagittaras.Scripting.Registries;
using Sagittaras.Scripting.Activation;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sagittaras.Scripting.Attributes;

namespace Sagittaras.Scripting.Building
{
    /// <inheritdoc />
    public class ScriptRegistryBuilder : IScriptRegistryBuilder
    {
        /// <summary>
        ///     Resolves the entrypoints of the registered scripts.
        /// </summary>
        private readonly IScriptHierarchy _hierarchy;

        /// <summary>
        ///     Global scripts collected per entrypoint, ordered once the manager is built.
        /// </summary>
        private readonly Dictionary<Type, List<GlobalScriptRegistration>> _globals = new();

        /// <summary>
        ///     Scripts shared by all instances of an entity.
        /// </summary>
        private readonly Dictionary<EntityScriptKey, IEntityScript> _entities = new();

        /// <summary>
        ///     Scripts created per instance of an entity.
        /// </summary>
        private readonly Dictionary<EntityScriptKey, TransientScriptRegistration> _transients = new();

        /// <summary>
        ///     Provides the transient script instances of the built manager.
        /// </summary>
        private IScriptActivator _activator = new PoolingScriptActivator();

        /// <summary>
        ///     Number of global scripts registered so far, used to break the ties of equal priorities.
        /// </summary>
        private int _order;

        /// <summary>
        ///     Creates the builder resolving the entrypoints on its own.
        /// </summary>
        public ScriptRegistryBuilder() : this(new ScriptHierarchy())
        {
        }

        /// <summary>
        ///     Creates the builder over the given resolver of entrypoints.
        /// </summary>
        /// <param name="hierarchy">Resolver of the entrypoints a script is registered under.</param>
        public ScriptRegistryBuilder(IScriptHierarchy hierarchy)
        {
            _hierarchy = hierarchy;
        }

        /// <inheritdoc />
        public IScriptRegistryBuilder AddGlobal(IGlobalScript script)
        {
            IReadOnlyList<Type> entrypoints = RequireEntrypoints(script.GetType());
            GlobalScriptRegistration registration = new(script, ReadPriority(script.GetType()), _order++);

            for (int i = 0; i < entrypoints.Count; i++)
            {
                if (!_globals.TryGetValue(entrypoints[i], out List<GlobalScriptRegistration>? bucket))
                {
                    bucket = new List<GlobalScriptRegistration>();
                    _globals.Add(entrypoints[i], bucket);
                }

                bucket.Add(registration);
            }

            return this;
        }

        /// <inheritdoc />
        public IScriptRegistryBuilder AddEntity(string entityId, IEntityScript script)
        {
            IReadOnlyList<Type> entrypoints = RequireEntrypoints(script.GetType());
            RequireFreeEntrypoints(entityId, entrypoints, _entities.ContainsKey);

            for (int i = 0; i < entrypoints.Count; i++)
            {
                _entities.Add(new EntityScriptKey(entityId, entrypoints[i]), script);
            }

            return this;
        }

        /// <inheritdoc />
        public IScriptRegistryBuilder AddTransient<TScript>(string entityId) where TScript : class, ITransientEntityScript, new()
        {
            IReadOnlyList<Type> entrypoints = RequireEntrypoints(typeof(TScript));
            RequireFreeEntrypoints(entityId, entrypoints, _transients.ContainsKey);

            TransientScriptRegistration registration = new(Activate<TScript>, entrypoints);
            for (int i = 0; i < entrypoints.Count; i++)
            {
                _transients.Add(new EntityScriptKey(entityId, entrypoints[i]), registration);
            }

            return this;
        }

        /// <inheritdoc />
        public IScriptRegistryBuilder WithActivator(IScriptActivator activator)
        {
            _activator = activator;
            return this;
        }

        /// <inheritdoc />
        public IScriptManager Build()
        {
            Dictionary<Type, object> globals = new(_globals.Count);
            foreach (KeyValuePair<Type, List<GlobalScriptRegistration>> entry in _globals)
            {
                globals.Add(entry.Key, CreateBucket(entry.Key, entry.Value));
            }

            return new ScriptManager(
                new GlobalScriptRegistry(globals),
                new EntityScriptRegistry(new Dictionary<EntityScriptKey, IEntityScript>(_entities)),
                new TransientEntityScriptRegistry(new Dictionary<EntityScriptKey, TransientScriptRegistration>(_transients), _activator)
            );
        }

        /// <summary>
        ///     Takes an instance of the script from the activator.
        /// </summary>
        /// <remarks>
        ///     Captured while the concrete type is still known statically, which is what leaves the spawn
        ///     itself free of reflection.
        /// </remarks>
        /// <param name="activator">Activator providing the instance.</param>
        /// <typeparam name="TScript">Concrete type of the script.</typeparam>
        /// <returns>Returns the instance to be initialized.</returns>
        private static ITransientEntityScript Activate<TScript>(IScriptActivator activator) where TScript : class, ITransientEntityScript, new()
        {
            return activator.Create<TScript>();
        }

        /// <summary>
        ///     Builds the ordered bucket of scripts of a single entrypoint.
        /// </summary>
        /// <remarks>
        ///     The bucket is a <see cref="List{T}"/> of the entrypoint type, so that a lookup casts it as a
        ///     whole instead of casting every script it holds.
        /// </remarks>
        /// <param name="entrypoint">Entrypoint the scripts are reached through.</param>
        /// <param name="registrations">Scripts registered under the entrypoint.</param>
        /// <returns>Returns the bucket ordered by the priority of the scripts.</returns>
        private static object CreateBucket(Type entrypoint, List<GlobalScriptRegistration> registrations)
        {
            registrations.Sort(CompareByPriority);

            IList bucket = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(entrypoint))!;
            for (int i = 0; i < registrations.Count; i++)
            {
                bucket.Add(registrations[i].Script);
            }

            return bucket;
        }

        /// <summary>
        ///     Orders the scripts of an entrypoint, the highest priority first.
        /// </summary>
        /// <param name="left">Registration on the left side of the comparison.</param>
        /// <param name="right">Registration on the right side of the comparison.</param>
        /// <returns>Returns the relative order of the two registrations.</returns>
        private static int CompareByPriority(GlobalScriptRegistration left, GlobalScriptRegistration right)
        {
            int byPriority = right.Priority.CompareTo(left.Priority);
            return byPriority != 0 ? byPriority : left.Order.CompareTo(right.Order);
        }

        /// <summary>
        ///     Reads the priority declared by the script.
        /// </summary>
        /// <param name="scriptType">Type of the concrete script.</param>
        /// <returns>Returns the declared priority, or <see cref="ScriptPriorityAttribute.DefaultPriority"/>.</returns>
        private static int ReadPriority(Type scriptType)
        {
            return scriptType.GetCustomAttribute<ScriptPriorityAttribute>()?.Priority ?? ScriptPriorityAttribute.DefaultPriority;
        }

        /// <summary>
        ///     Resolves the entrypoints of a script that is being registered.
        /// </summary>
        /// <param name="scriptType">Type of the concrete script.</param>
        /// <returns>Returns the entrypoints the script is registered under.</returns>
        /// <exception cref="ArgumentException">Thrown when the script derives from no entrypoint, as nothing could ever reach it.</exception>
        private IReadOnlyList<Type> RequireEntrypoints(Type scriptType)
        {
            IReadOnlyList<Type> entrypoints = _hierarchy.GetEntrypoints(scriptType);

            return entrypoints.Count == 0 
                ? throw new ArgumentException($"Script `{scriptType}` derives from no type annotated by `{nameof(GameScriptAttribute)}`, so no hook-point could ever reach it.", nameof(scriptType)) 
                : entrypoints;
        }

        /// <summary>
        ///     Verifies that the entity is served by no script under any of the entrypoints yet.
        /// </summary>
        /// <remarks>
        ///     Checked for all entrypoints before anything is registered, so that a refused registration
        ///     leaves the builder untouched.
        /// </remarks>
        /// <param name="entityId">Identifier of the entity the script is registered for.</param>
        /// <param name="entrypoints">Entrypoints the script would be registered under.</param>
        /// <param name="isTaken">Tells whether a script is already registered for the key.</param>
        /// <exception cref="ArgumentException">Thrown when the entity already has a script under one of the entrypoints.</exception>
        private static void RequireFreeEntrypoints(string entityId, IReadOnlyList<Type> entrypoints, Func<EntityScriptKey, bool> isTaken)
        {
            for (int i = 0; i < entrypoints.Count; i++)
            {
                if (isTaken(new EntityScriptKey(entityId, entrypoints[i])))
                {
                    throw new ArgumentException($"Entity `{entityId}` is already served by a script reached through `{entrypoints[i]}`.", nameof(entityId));
                }
            }
        }
    }
}
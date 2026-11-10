using System;
using Sagittaras.Scripting.Activation;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sagittaras.Scripting.Entities;

namespace Sagittaras.Scripting.Registries
{
    /// <inheritdoc />
    internal class TransientEntityScriptRegistry : ITransientEntityScriptRegistry
    {
        /// <summary>
        ///     How to create the script of each pair of an entity and an entrypoint.
        /// </summary>
        private readonly Dictionary<EntityScriptKey, TransientScriptRegistration> _registrations;

        /// <summary>
        ///     Provides and takes back the script instances.
        /// </summary>
        private readonly IScriptActivator _activator;

        /// <summary>
        ///     Scripts of the entity instances currently alive.
        /// </summary>
        private readonly Dictionary<ScriptedEntityKey, ScriptedEntityInstance> _instances = new();

        /// <summary>
        ///     Distinct scripts of a single entity instance, reused by every release.
        /// </summary>
        /// <remarks>
        ///     A script is indexed under each of its entrypoints, so releasing an entity would hand the same
        ///     instance to the activator several times over.
        /// </remarks>
        private readonly HashSet<ITransientEntityScript> _released = new();

        /// <summary>
        ///     Handler subscribed to the entities holding a script.
        /// </summary>
        /// <remarks>
        ///     Created once and reused, so that subscribing on a spawn does not allocate a delegate.
        /// </remarks>
        private readonly EventHandler<ITransientScriptedEntity> _releasedHandler;

        internal TransientEntityScriptRegistry(Dictionary<EntityScriptKey, TransientScriptRegistration> registrations, IScriptActivator activator)
        {
            _registrations = registrations;
            _activator = activator;
            _releasedHandler = OnEntityReleased;
        }

        /// <inheritdoc />
        public void Instantiate<TGameScript, TArgs>(ITransientScriptedEntity entity, in TArgs args) where TGameScript : class, ITransientEntityScript<TArgs>
        {
            if (!_registrations.TryGetValue(new EntityScriptKey(entity.Id, typeof(TGameScript)), out TransientScriptRegistration registration))
            {
                return;
            }

            ITransientEntityScript script = registration.Factory(_activator);
            ((ITransientEntityScript<TArgs>)script).Initialize(entity, in args);

            Index(entity, script, registration.Entrypoints);
        }

        /// <inheritdoc />
        public void Instantiate<TGameScript>(ITransientScriptedEntity entity) where TGameScript : class, ITransientEntityScript<NoArgs>
        {
            NoArgs args = default;
            Instantiate<TGameScript, NoArgs>(entity, in args);
        }

        /// <inheritdoc />
        public bool TryGet<TGameScript>(ITransientScriptedEntity entity, [MaybeNullWhen(false)] out TGameScript script) where TGameScript : class, ITransientEntityScript
        {
            if (!_instances.TryGetValue(new ScriptedEntityKey(entity), out ScriptedEntityInstance? instance))
            {
                script = null;
                return false;
            }

            if (!instance.Scripts.TryGetValue(typeof(TGameScript), out ITransientEntityScript? found))
            {
                script = null;
                return false;
            }

            script = (TGameScript)found;
            return true;
        }

        /// <inheritdoc />
        public void Release(in ScriptedEntityKey key)
        {
            if (!_instances.TryGetValue(key, out ScriptedEntityInstance? instance))
            {
                return;
            }

            _instances.Remove(key);
            instance.Entity.Released -= _releasedHandler;

            _released.Clear();
            foreach (KeyValuePair<Type, ITransientEntityScript> entry in instance.Scripts)
            {
                if (_released.Add(entry.Value))
                {
                    _activator.Release(entry.Value);
                }
            }

            _released.Clear();
        }

        /// <summary>
        ///     Indexes the created script under each of its entrypoints.
        /// </summary>
        /// <param name="entity">Entity instance the script serves.</param>
        /// <param name="script">Script that has just been created.</param>
        /// <param name="entrypoints">Entrypoints the script is reachable through.</param>
        /// <exception cref="InvalidOperationException">Thrown when the entity instance already holds a script under one of the entrypoints.</exception>
        private void Index(ITransientScriptedEntity entity, ITransientEntityScript script, IReadOnlyList<Type> entrypoints)
        {
            ScriptedEntityKey key = new(entity);
            if (!_instances.TryGetValue(key, out ScriptedEntityInstance? instance))
            {
                instance = new ScriptedEntityInstance(entity);
                _instances.Add(key, instance);
                entity.Released += _releasedHandler;
            }

            for (int i = 0; i < entrypoints.Count; i++)
            {
                if (instance.Scripts.ContainsKey(entrypoints[i]))
                {
                    throw new InvalidOperationException($"Entity instance `{key}` already holds a transient script reached through `{entrypoints[i]}`.");
                }
            }

            for (int i = 0; i < entrypoints.Count; i++)
            {
                instance.Scripts.Add(entrypoints[i], script);
            }
        }

        /// <summary>
        ///     Drops the scripts of an entity that announced the end of its lifecycle.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="entity">Entity being released.</param>
        private void OnEntityReleased(object? sender, ITransientScriptedEntity entity)
        {
            Release(new ScriptedEntityKey(entity));
        }
    }
}
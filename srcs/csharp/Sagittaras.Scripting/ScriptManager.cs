using System;
using Sagittaras.Scripting.Building;
using System.Collections.Generic;
using Sagittaras.Scripting.Entities;
using Sagittaras.Scripting.Registries;

namespace Sagittaras.Scripting
{
    /// <inheritdoc />
    /// <remarks>
    ///     Dispatching only - the scripts themselves are held by the registries. Use
    ///     <see cref="ScriptRegistryBuilder"/> to register them and create the manager.
    /// </remarks>
    public class ScriptManager : IScriptManager
    {
        private readonly IGlobalScriptRegistry _globals;
        private readonly IEntityScriptRegistry _entities;
        private readonly ITransientEntityScriptRegistry _transients;

        /// <summary>
        ///     Creates the manager over the registries holding the registered scripts.
        /// </summary>
        /// <param name="globals">Registry of the scripts without an entity target.</param>
        /// <param name="entities">Registry of the scripts shared by all instances of an entity.</param>
        /// <param name="transients">Registry of the scripts created per instance of an entity.</param>
        internal ScriptManager(IGlobalScriptRegistry globals, IEntityScriptRegistry entities, ITransientEntityScriptRegistry transients)
        {
            _globals = globals;
            _entities = entities;
            _transients = transients;
        }

        /// <inheritdoc />
        public void Instantiate<TGameScript, TArgs>(ITransientScriptedEntity entity, in TArgs args) where TGameScript : class, ITransientEntityScript<TArgs>
        {
            _transients.Instantiate<TGameScript, TArgs>(entity, in args);
        }

        /// <inheritdoc />
        public void Instantiate<TGameScript>(ITransientScriptedEntity entity) where TGameScript : class, ITransientEntityScript<NoArgs>
        {
            _transients.Instantiate<TGameScript>(entity);
        }

        /// <inheritdoc />
        public void Call<TGameScript>(Action<TGameScript> hook) where TGameScript : class, IGlobalScript
        {
            IReadOnlyList<TGameScript> scripts = _globals.Get<TGameScript>();
            for (int i = 0; i < scripts.Count; i++)
            {
                hook(scripts[i]);
            }
        }

        /// <inheritdoc />
        public TResult? Call<TGameScript, TResult>(Func<TGameScript, TResult> hook, TResult? @default = default) where TGameScript : class, IGlobalScript
        {
            IReadOnlyList<TGameScript> scripts = _globals.Get<TGameScript>();
            for (int i = 0; i < scripts.Count; i++)
            {
                TResult result = hook(scripts[i]);
                if (!EqualityComparer<TResult?>.Default.Equals(result, default))
                {
                    return result;
                }
            }

            return @default;
        }

        /// <inheritdoc />
        public void Call<TGameScript>(IScriptedEntity entity, Action<TGameScript> hook) where TGameScript : class, IEntityScript
        {
            if (_entities.TryGet<TGameScript>(entity.Id, out TGameScript? script))
            {
                hook(script);
            }
        }

        /// <inheritdoc />
        public TResult? Call<TGameScript, TResult>(IScriptedEntity entity, Func<TGameScript, TResult> hook, TResult? @default = default) where TGameScript : class, IEntityScript
        {
            if (!_entities.TryGet<TGameScript>(entity.Id, out TGameScript? script))
            {
                return @default;
            }

            return hook(script);
        }

        /// <inheritdoc />
        public void Call<TGameScript>(ITransientScriptedEntity entity, Action<TGameScript> hook) where TGameScript : class, ITransientEntityScript
        {
            if (_transients.TryGet<TGameScript>(entity, out TGameScript? script))
            {
                hook(script);
            }
        }

        /// <inheritdoc />
        public TResult? Call<TGameScript, TResult>(ITransientScriptedEntity entity, Func<TGameScript, TResult> hook, TResult? @default = default) where TGameScript : class, ITransientEntityScript
        {
            if (!_transients.TryGet<TGameScript>(entity, out TGameScript? script))
            {
                return @default;
            }

            return hook(script);
        }

        /// <inheritdoc />
        public bool HasScript<TGameScript>() where TGameScript : class, IGlobalScript
        {
            return _globals.Get<TGameScript>().Count > 0;
        }

        /// <inheritdoc />
        public IReadOnlyList<TGameScript> GetScripts<TGameScript>() where TGameScript : class, IGlobalScript
        {
            return _globals.Get<TGameScript>();
        }

        /// <inheritdoc />
        public bool HasScript<TGameScript>(IScriptedEntity entity) where TGameScript : class, IEntityScript
        {
            return _entities.TryGet<TGameScript>(entity.Id, out TGameScript? _);
        }

        /// <inheritdoc />
        public TGameScript? GetScript<TGameScript>(IScriptedEntity entity) where TGameScript : class, IEntityScript
        {
            return _entities.TryGet<TGameScript>(entity.Id, out TGameScript? script) ? script : null;
        }

        /// <inheritdoc />
        public bool HasScript<TGameScript>(ITransientScriptedEntity entity) where TGameScript : class, ITransientEntityScript
        {
            return _transients.TryGet<TGameScript>(entity, out TGameScript? _);
        }

        /// <inheritdoc />
        public TGameScript? GetScript<TGameScript>(ITransientScriptedEntity entity) where TGameScript : class, ITransientEntityScript
        {
            return _transients.TryGet<TGameScript>(entity, out TGameScript? script) ? script : null;
        }
    }
}

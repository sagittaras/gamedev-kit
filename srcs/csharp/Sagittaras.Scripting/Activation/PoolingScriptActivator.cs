using System;
using System.Collections.Generic;

namespace Sagittaras.Scripting.Activation
{
    /// <inheritdoc />
    /// <remarks>
    ///     <para>
    ///         Keeps the released scripts and hands them out again instead of letting every spawn allocate.
    ///         It works because a transient script takes its state in
    ///         <see cref="ITransientEntityScript{TArgs}.Initialize"/> rather than through its constructor -
    ///         a rented instance is bound to its new entity the same way a fresh one is.
    ///     </para>
    ///     <para>
    ///         The scripts it holds are the ones already released, so the pool settles at the peak number of
    ///         entities that lived at once. Pass a capacity where that peak is not something the game keeps
    ///         a lid on by itself.
    ///     </para>
    ///     <para>
    ///         This is what a manager uses unless told otherwise, so that transient scripts cost what they
    ///         should out of the box. A capacity of zero turns the pooling off and every spawn allocates
    ///         again, which is the way to tell a leftover state apart from a defect while chasing one.
    ///     </para>
    ///     <para>
    ///         Not safe for concurrent use, the same as the registry that drives it.
    ///     </para>
    /// </remarks>
    public class PoolingScriptActivator : IScriptActivator
    {
        /// <summary>
        ///     Capacity of a pool that keeps every script released to it.
        /// </summary>
        public const int Unbounded = int.MaxValue;

        /// <summary>
        ///     Released scripts waiting to be handed out again, per concrete type.
        /// </summary>
        /// <remarks>
        ///     A stack rather than a queue - the instance released last is the one still warm in the cache.
        /// </remarks>
        private readonly Dictionary<Type, Stack<ITransientEntityScript>> _pools = new();

        /// <summary>
        ///     Scripts currently held by the pools.
        /// </summary>
        /// <remarks>
        ///     Releasing a script twice would hand the same instance to two entities at once, and neither of
        ///     them would show anything but strange behavior. The set is what turns that into a report.
        /// </remarks>
        private readonly HashSet<ITransientEntityScript> _pooled = new(ScriptReferenceComparer.Instance);

        /// <summary>
        ///     Number of scripts a single pool keeps, zero to pool nothing at all, or <see cref="Unbounded"/>.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Creates the activator keeping every script released to it.
        /// </summary>
        public PoolingScriptActivator() : this(Unbounded)
        {
        }

        /// <summary>
        ///     Creates the activator keeping at most the given number of scripts of each type.
        /// </summary>
        /// <param name="capacity">Number of scripts a single pool keeps, zero to pool nothing at all, or <see cref="Unbounded"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the capacity is negative.</exception>
        public PoolingScriptActivator(int capacity)
        {
            _capacity = capacity >= 0
                ? capacity
                : throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity of a pool cannot be negative.");
        }

        /// <inheritdoc />
        /// <remarks>
        ///     A rented instance carries the state of the entity it served before, which
        ///     <see cref="ITransientEntityScript{TArgs}.Initialize"/> is expected to overwrite in full.
        /// </remarks>
        public TScript Create<TScript>() where TScript : class, ITransientEntityScript, new()
        {
            if (!_pools.TryGetValue(typeof(TScript), out Stack<ITransientEntityScript>? pool) || pool.Count == 0)
            {
                return new TScript();
            }

            ITransientEntityScript script = pool.Pop();
            _pooled.Remove(script);

            return (TScript)script;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Thrown when the script is already held by the pool.</exception>
        public void Release(ITransientEntityScript script)
        {
            if (_pooled.Contains(script))
            {
                throw new InvalidOperationException($"Script `{script.GetType()}` has been released twice, so the pool would hand the very same instance to two entities at once.");
            }

            Type scriptType = script.GetType();
            _pools.TryGetValue(scriptType, out Stack<ITransientEntityScript>? pool);

            if ((pool?.Count ?? 0) >= _capacity)
            {
                return;
            }

            if (pool is null)
            {
                pool = new Stack<ITransientEntityScript>();
                _pools.Add(scriptType, pool);
            }

            _pooled.Add(script);
            pool.Push(script);
        }

        /// <summary>
        ///     Fills the pool of a script ahead of the spawns that would otherwise allocate it.
        /// </summary>
        /// <remarks>
        ///     Worth doing while a scene loads, so that the first wave of entities costs no more than the
        ///     later ones. Stops at the capacity of the pool.
        /// </remarks>
        /// <param name="count">Number of instances to create in advance.</param>
        /// <typeparam name="TScript">Concrete type of the script to create.</typeparam>
        public void Prewarm<TScript>(int count) where TScript : class, ITransientEntityScript, new()
        {
            for (int i = 0; i < count; i++)
            {
                Release(new TScript());
            }
        }

        /// <summary>
        ///     Number of scripts of the type the pool can hand out without allocating.
        /// </summary>
        /// <param name="scriptType">Concrete type of the script.</param>
        /// <returns>Returns the number of scripts waiting in the pool.</returns>
        public int Available(Type scriptType)
        {
            return _pools.TryGetValue(scriptType, out Stack<ITransientEntityScript>? pool) ? pool.Count : 0;
        }

        /// <summary>
        ///     Drops every script the pools hold.
        /// </summary>
        /// <remarks>
        ///     Scripts of a scene that has been unloaded may hold on to what the scene left behind, and the
        ///     pool would keep them alive until the type spawns again.
        /// </remarks>
        public void Clear()
        {
            _pools.Clear();
            _pooled.Clear();
        }
    }
}
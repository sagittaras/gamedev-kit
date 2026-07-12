using System;
using System.Reflection;

namespace Sagittaras.Messaging.Subscribers
{
    /// <inheritdoc />
    public readonly struct MediatorSubscriber<TContract> : IMediatorSubscriber
        where TContract : IMediatorContract
    {
        private readonly Action<TContract> _callback;

        /// <summary>
        ///     Creates a new wrapper for the given callback.
        /// </summary>
        /// <param name="callback">Action callback for a given contract type.</param>
        public MediatorSubscriber(Action<TContract> callback)
        {
            _callback = callback;
            ContractType = typeof(TContract);
            Method = callback.Method;
        }

        /// <inheritdoc />
        public Type ContractType { get; }

        /// <inheritdoc />
        public MethodInfo Method { get; }

        /// <inheritdoc />
        public void Invoke(IMediatorContract contract)
        {
            Invoke((TContract)contract);
        }

        private void Invoke(TContract contract)
        {
            _callback.Invoke(contract);
        }

        /// <inheritdoc />
        public bool Equals(IMediatorSubscriber other)
        {
            return other.GetHashCode().Equals(GetHashCode());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is IMediatorSubscriber other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _callback.GetHashCode();
        }
    }
}
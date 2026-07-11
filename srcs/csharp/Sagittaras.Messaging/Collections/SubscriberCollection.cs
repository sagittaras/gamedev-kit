using System;
using System.Collections;
using System.Collections.Generic;
using Sagittaras.Messaging.Subscribers;

namespace Sagittaras.Messaging.Collections
{
    /// <summary>
    ///     Maintains a collection of subscribers to <see cref="IMediatorContract"/> types.
    /// </summary>
    internal class SubscriberCollection : IEnumerable<IMediatorSubscriber>
    {
        /// <summary>
        ///     Map of <see cref="IMediatorSubscriber"/> by the type of their contract.
        /// </summary>
        private readonly Dictionary<Type, List<IMediatorSubscriber>> _subscribers = new(16);

        /// <summary>
        ///     Map of <see cref="IMediatorSubscriber"/> by their hash code.
        /// </summary>
        private readonly Dictionary<int, IMediatorSubscriber> _subscribersByHashCode = new(16);

        /// <summary>
        ///     Total number of registered subscribers.
        /// </summary>
        public int Count => _subscribersByHashCode.Count;

        /// <summary>
        ///     Adds a new <see cref="IMediatorSubscriber"/> to the collection.
        /// </summary>
        /// <param name="subscriber">Instance of a new subscriber to be added.</param>
        public void Add(IMediatorSubscriber subscriber)
        {
            if (_subscribersByHashCode.ContainsKey(subscriber.GetHashCode()))
            {
                return;
            }

            if (!_subscribers.TryGetValue(subscriber.ContractType, out List<IMediatorSubscriber>? subscribers))
            {
                subscribers = new List<IMediatorSubscriber>();
                _subscribers.Add(subscriber.ContractType, subscribers);
            }

            subscribers.Add(subscriber);
            _subscribersByHashCode.Add(subscriber.GetHashCode(), subscriber);
        }

        /// <summary>
        ///     Removes a subscriber from the collection based on its callback delegate.
        /// </summary>
        /// <param name="callback">Callback delegate to recognize subscriber.</param>
        public void Remove(Delegate callback)
        {
            if (!_subscribersByHashCode.TryGetValue(callback.GetHashCode(), out IMediatorSubscriber? subscriber))
            {
                return;
            }

            _subscribersByHashCode.Remove(callback.GetHashCode());

            if (!_subscribers.TryGetValue(subscriber.ContractType, out List<IMediatorSubscriber> subscribers))
            {
                return;
            }

            subscribers.Remove(subscriber);
        }

        public ContractSubscriberCollection Get(Type contractType)
        {
            return _subscribers.TryGetValue(contractType, out List<IMediatorSubscriber>? subscribers)
                ? new ContractSubscriberCollection(subscribers.AsReadOnly())
                : ContractSubscriberCollection.Empty;
        }

        /// <inheritdoc />
        public IEnumerator<IMediatorSubscriber> GetEnumerator()
        {
            return _subscribersByHashCode.Values.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
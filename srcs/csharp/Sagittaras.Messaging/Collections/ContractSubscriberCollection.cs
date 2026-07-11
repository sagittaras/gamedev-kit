using System;
using System.Collections;
using System.Collections.Generic;
using Sagittaras.Messaging.Subscribers;

namespace Sagittaras.Messaging.Collections
{
    /// <summary>
    ///     Result of <see cref="SubscriberCollection.Get"/>, containing all registered subscribers for a given contract.
    /// </summary>
    internal readonly struct ContractSubscriberCollection : IEnumerable<IMediatorSubscriber>
    {
        /// <summary>
        ///     Represents an empty collection of subscribers.
        /// </summary>
        public static readonly ContractSubscriberCollection Empty = new(new List<IMediatorSubscriber>());
        
        private readonly IReadOnlyCollection<IMediatorSubscriber> _subscribers;
        
        public ContractSubscriberCollection(IReadOnlyCollection<IMediatorSubscriber> subscribers)
        {
            _subscribers = subscribers;
        }
        
        /// <summary>
        ///     Number of available subscribers in the collection.
        /// </summary>
        public int Count => _subscribers.Count;

        /// <summary>
        ///     Invokes all subscribers for the given contract.
        /// </summary>
        /// <param name="contract">Contract containing context of the action.</param>
        /// <param name="onException">Action callback allowing processing of subscriber exceptions.</param>
        public void Invoke(IMediatorContract contract, Action<SubscriberException> onException)
        {
            foreach (IMediatorSubscriber? subscriber in _subscribers)
            {
                try
                {
                    subscriber.Invoke(contract);
                }
                catch (Exception e)
                {
                    onException.Invoke(new SubscriberException(contract, subscriber, e));
                }
            }
        }

        /// <inheritdoc />
        public IEnumerator<IMediatorSubscriber> GetEnumerator()
        {
            return _subscribers.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
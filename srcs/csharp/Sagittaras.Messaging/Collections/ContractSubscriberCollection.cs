using System;
using System.Collections.Generic;
using Sagittaras.Messaging.Subscribers;

namespace Sagittaras.Messaging.Collections
{
    /// <summary>
    ///     Result of <see cref="SubscriberCollection.Get"/>, containing all registered subscribers for a given contract.
    /// </summary>
    internal readonly struct ContractSubscriberCollection
    {
        private readonly IReadOnlyCollection<IMediatorSubscriber> _subscribers;
        
        public ContractSubscriberCollection(IReadOnlyCollection<IMediatorSubscriber> subscribers)
        {
            _subscribers = subscribers;
        }

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
    }
}
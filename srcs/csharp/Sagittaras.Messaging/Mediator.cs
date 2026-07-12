using System;
using Sagittaras.Messaging.Collections;
using Sagittaras.Messaging.Subscribers;

namespace Sagittaras.Messaging
{
    /// <inheritdoc />
    public class Mediator : IMediator
    {
        /// <inheritdoc cref="SubscriberCollection" />
        private readonly SubscriberCollection _subscribers = new();

        /// <summary>
        ///     Event fired each time when a subscriber throws an exception during invocation.
        /// </summary>
        /// <remarks>
        ///     Helps with logging of the exceptions during runtime, as Mediator itself has no native support for runtime.
        /// </remarks>
        public static event EventHandler<SubscriberException>? ExceptionRaised;

        /// <summary>
        ///     Provides global access to the mediator for subscribing to, unsubscribing from, and publishing message contracts.
        /// </summary>
        public static IMediator Instance { get; } = new Mediator();
        
        /// <inheritdoc />
        public void Subscribe<TContract>(Action<TContract> callback) where TContract : IMediatorContract
        {
            _subscribers.Add(new MediatorSubscriber<TContract>(callback));
        }

        /// <inheritdoc />
        public void Unsubscribe<TContract>(Action<TContract> callback) where TContract : IMediatorContract
        {
            _subscribers.Remove(callback);
        }

        /// <inheritdoc />
        public void Publish<TContract>(TContract contract) where TContract : IMediatorContract
        {
            _subscribers.Get(typeof(TContract)).Invoke(contract, SubscriberOnException);
        }

        /// <summary>
        ///     Raises the <see cref="ExceptionRaised"/> event when a subscriber throws an exception during invocation.
        /// </summary>
        /// <param name="e">The <see cref="SubscriberException"/> instance containing information about the exception.</param>
        private void SubscriberOnException(SubscriberException e)
        {
            ExceptionRaised?.Invoke(this, e);
        }
    }
}
using System;

namespace Sagittaras.Messaging
{
    /// <summary>
    ///     Provides loosely coupled communication between game components via publish/subscribe contracts.
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        ///     Subscribes a callback to a mediator contract.
        /// </summary>
        /// <param name="callback">Callback to be invoked when the contract is received.</param>
        /// <typeparam name="TContract">Type of contract used in communication.</typeparam>
        void Subscribe<TContract>(Action<TContract> callback) where TContract : IMediatorContract;
        
        /// <summary>
        ///     Unsubscribes a callback from a mediator contract.
        /// </summary>
        /// <param name="callback">Callback to be unsubscribed.</param>
        /// <typeparam name="TContract">Type of contract used in communication.</typeparam>
        void Unsubscribe<TContract>(Action<TContract> callback) where TContract : IMediatorContract;

        /// <summary>
        ///     Publishes a contract to all subscribed callbacks.
        /// </summary>
        /// <param name="contract">The contract to be published to subscribers.</param>
        /// <typeparam name="TContract">Type of contract being published.</typeparam>
        void Publish<TContract>(TContract contract) where TContract : IMediatorContract;
    }
}
using System;
using System.Reflection;

namespace Sagittaras.Messaging.Subscribers
{
    /// <summary>
    ///     Describes a non-generic wrapper for a mediator subscriber.
    /// </summary>
    public interface IMediatorSubscriber : IEquatable<IMediatorSubscriber>
    {
        /// <summary>
        ///     Type of <see cref="IMediatorContract"/> served by this subscriber.
        /// </summary>
        Type ContractType { get; }
        
        /// <summary>
        ///     Gets information about the callback method associated with the subscriber.
        /// </summary>
        MethodInfo Method { get; }
        
        /// <summary>
        ///     Invokes the action callback associated with the given contract.
        /// </summary>
        /// <param name="contract">Contract carrying the context of mediator's action.</param>
        void Invoke(IMediatorContract contract);
    }
}
using System;
using System.Text;

namespace Sagittaras.Messaging.Subscribers
{
    /// <summary>
    ///     Represents an exception that occurs when a mediator subscriber encounters an error during invocation.
    /// </summary>
    /// <remarks>
    ///     This exception is thrown to encapsulate errors occurring in the execution of a subscriber's method.
    ///     It provides detailed context about the subscriber and the underlying exception.
    /// </remarks>
    public class SubscriberException : Exception
    {
        /// <summary>
        ///     Represents an exception that occurs when a mediator subscriber encounters an error during invocation.
        /// </summary>
        public SubscriberException(IMediatorContract contract, IMediatorSubscriber subscriber, Exception innerException) : base(GetExceptionMessage(subscriber, innerException), innerException)
        {
            Contract = contract;
            Subscriber = subscriber;
        }
        
        /// <summary>
        ///     Instance of <see cref="IMediatorContract"/> which caused the exception.
        /// </summary>
        public IMediatorContract Contract { get; }
        
        /// <summary>
        ///     Instance of <see cref="IMediatorSubscriber"/> which caused the exception.
        /// </summary>
        public IMediatorSubscriber Subscriber { get; }

        /// <summary>
        ///     Constructs a detailed exception message for a mediator subscriber that caused an exception during invocation.
        /// </summary>
        /// <param name="subscriber">The mediator subscriber that caused the exception.</param>
        /// <param name="innerException">The exception that was thrown during the invocation of the subscriber.</param>
        /// <returns>A string containing a detailed description of the exception, including the subscriber's identity and message from the inner exception.</returns>
        private static string GetExceptionMessage(IMediatorSubscriber subscriber, Exception innerException)
        {
            StringBuilder builder = new();
            string subscriberIdentity = subscriber.Method.DeclaringType is not null
                ? $"{subscriber.Method.DeclaringType.Name}.{subscriber.Method.Name}"
                : subscriber.Method.Name;
            
            builder.Append($"Subscriber `{subscriberIdentity}` caused an exception during invocation: {innerException.Message}");
            builder.Append(" See inner exception for more details.");
            
            return builder.ToString();
        }
    }
}
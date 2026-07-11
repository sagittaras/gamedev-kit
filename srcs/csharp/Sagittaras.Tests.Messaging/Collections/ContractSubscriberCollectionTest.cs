using AwesomeAssertions;
using AwesomeAssertions.Events;
using Sagittaras.Messaging;
using Sagittaras.Messaging.Collections;
using Sagittaras.Messaging.Subscribers;

namespace Sagittaras.Tests.Messaging.Collections;

public class ContractSubscriberCollectionTest
{
    /// <summary>
    ///     Verifies the invocation for a contract in a collection.
    /// </summary>
    [Fact]
    public void Invoke()
    {
        MySubscriber target = new();
        using IMonitor<MySubscriber> monitoredTarget = target.Monitor();
        ContractSubscriberCollection collection = new([
            new MediatorSubscriber<MyContract>(target.OnMyContract)
        ]);

        MyContract contract = new();
        collection.Invoke(contract, target.OnException);
        monitoredTarget.Should().Raise(nameof(MySubscriber.Invocation));
        monitoredTarget.Should().NotRaise(nameof(MySubscriber.InvocationException));
    }

    /// <summary>
    ///     Verifies the invocation returns an exception through the callback.
    /// </summary>
    [Fact]
    public void Invoke_CatchException()
    {
        MySubscriber target = new();
        using IMonitor<MySubscriber> monitoredTarget = target.Monitor();
        ContractSubscriberCollection collection = new([
            new MediatorSubscriber<MyContract>(target.OnMyContract_Fails)
        ]);

        MyContract contract = new();
        collection.Invoke(contract, target.OnException);
        monitoredTarget.Should().Raise(nameof(MySubscriber.InvocationException));
    }

    private record MyContract : IMediatorContract;

    private class MySubscriber
    {
        /// <summary>
        ///     Event used to monitor invocation of susbcriber method.
        /// </summary>
        public event EventHandler Invocation;

        /// <summary>
        ///     Event used to monitor invocation exceptions.
        /// </summary>
        public event EventHandler InvocationException;

        /// <summary>
        ///     Simple subscriber method verifying invocation.
        /// </summary>
        public void OnMyContract(MyContract contract)
        {
            Invocation.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Throws an exception as a simulation of failed subscriber invocation.
        /// </summary>
        public void OnMyContract_Fails(MyContract contract)
        {
            throw new Exception("Simulation of exception during invocation");
        }

        /// <summary>
        ///     Raises the <see cref="InvocationException"/> event, when an exception occurs during invocation.
        /// </summary>
        /// <param name="e"></param>
        public void OnException(Exception e)
        {
            InvocationException.Invoke(this, EventArgs.Empty);
        }
    }
}
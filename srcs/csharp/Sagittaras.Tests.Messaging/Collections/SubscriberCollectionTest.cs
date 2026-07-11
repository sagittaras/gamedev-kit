using AwesomeAssertions;
using Sagittaras.Messaging;
using Sagittaras.Messaging.Collections;
using Sagittaras.Messaging.Subscribers;

namespace Sagittaras.Tests.Messaging.Collections;

public class SubscriberCollectionTest
{
    /// <summary>
    ///     Verifies whether the subscriber method can be recognized simply by its HashCode,
    ///     since it should not be possible to add the same method twice.
    /// </summary>
    [Fact]
    public void AddCallback()
    {
        DummySubscriber dummy = new();
        SubscriberCollection collection =
        [
            new MediatorSubscriber<DummyContract>(dummy.OnDummyContract)
        ];

        collection.Count.Should().Be(1);
        
        collection.Add(new MediatorSubscriber<DummyContract>(dummy.OnDummyContract));
        collection.Count.Should().Be(1); // Same method should not be added twice.
    }

    /// <summary>
    ///     Verifies whether the subscriber method can be removed from the collection simply
    ///     by its HashCode.
    /// </summary>
    [Fact]
    public void RemoveCallback()
    {
        DummySubscriber dummy = new();
        SubscriberCollection collection =
        [
            new MediatorSubscriber<DummyContract>(dummy.OnDummyContract)
        ];

        collection.Count.Should().Be(1);
        
        collection.Remove(dummy.OnDummyContract);
        collection.Count.Should().Be(0);
    }

    /// <summary>
    ///     Verifies whether the collection returns the subscriber.
    /// </summary>
    [Fact]
    public void Get()
    {
        DummySubscriber dummy = new();
        SubscriberCollection collection =
        [
            new MediatorSubscriber<DummyContract>(dummy.OnDummyContract)
        ];

        ContractSubscriberCollection get = collection.Get(typeof(DummyContract));
        get.Should().NotBeNull();
        get.Count.Should().Be(1);

        IMediatorSubscriber sub = get.First();
        sub.Method.Name.Should().Be(nameof(DummySubscriber.OnDummyContract));
    }

    /// <summary>
    ///     Verifies whether the collection returns an empty collection for an unassigned subscriber.
    /// </summary>
    [Fact]
    public void Get_UnassignedSubscriber()
    {
        SubscriberCollection collection = [];
        
        ContractSubscriberCollection get = collection.Get(typeof(UnusedContract));
        get.Should().NotBeNull();
        get.Count.Should().Be(0);
    }

    /// <summary>
    ///     A dummy contract used as a subscriber's signature.
    /// </summary>
    private record DummyContract : IMediatorContract;
    
    /// <summary>
    ///     A dummy contract used for testing unassigned subscriber.
    /// </summary>
    private record UnusedContract : IMediatorContract;
    
    /// <summary>
    ///     A simple class providing dummy subscriber methods.
    /// </summary>
    private class DummySubscriber
    {
        /// <summary>
        ///     Dummy subscriber method for a simple contract.
        /// </summary>
        public void OnDummyContract(DummyContract contract)
        {
        }
    }
}
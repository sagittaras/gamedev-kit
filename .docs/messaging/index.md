# Sagittaras.Messaging

`Sagittaras.Messaging` is a Mediator / Pub-Sub library for loosely coupled communication between game components.
Instead of components holding direct references to each other, they exchange **contracts** — plain marker types —
through a central `IMediator`. Publishers don't need to know who (if anyone) is listening, and subscribers don't
need to know who published.

## Dependencies

This package has no dependencies on other packages from the kit.

## Getting Started

Communication happens through three pieces: a **contract** (the message), an `IMediator` instance, and one or
more **subscribers** (callbacks).

```csharp
// 1. Define a contract - a plain marker type implementing IMediatorContract.
public record PlayerDied(string PlayerName) : IMediatorContract;

// 2. Subscribe to it - use the global instance, or your own Mediator (see below).
IMediator mediator = Mediator.Instance;
mediator.Subscribe<PlayerDied>(OnPlayerDied);

void OnPlayerDied(PlayerDied contract)
{
    Debug.Log($"{contract.PlayerName} has died.");
}

// 3. Publish it - every subscriber for PlayerDied is invoked synchronously.
mediator.Publish(new PlayerDied("Hero"));

// 4. Unsubscribe when no longer needed (e.g. on component teardown).
mediator.Unsubscribe<PlayerDied>(OnPlayerDied);
```

`record` types are a convenient choice for contracts since they give you value equality and immutability for free,
but any type implementing `IMediatorContract` works — including plain classes or structs.

## Types

### `IMediatorContract`

An empty marker interface. Implementing it is what makes a type usable as a message on the mediator. It carries
no members — its only purpose is to constrain `Subscribe`, `Unsubscribe` and `Publish` to intentional message
types instead of `object`.

```csharp
public record DamageDealt(int Amount, GameObject Target) : IMediatorContract
{
    public int Amount { get; } = Amount;
    public GameObject Target { get; } = Target;
}
```

### `IMediator` / `Mediator`

The entry point for all pub/sub operations. `Mediator` is the default implementation.

```csharp
public interface IMediator
{
    void Subscribe<TContract>(Action<TContract> callback) where TContract : IMediatorContract;
    void Unsubscribe<TContract>(Action<TContract> callback) where TContract : IMediatorContract;
    void Publish<TContract>(TContract contract) where TContract : IMediatorContract;
}
```

`Mediator.Instance` exposes a global, shared singleton — the simplest way to get every component talking to the
same mediator without wiring it through dependency injection:

```csharp
Mediator.Instance.Subscribe<PlayerDied>(OnPlayerDied);
Mediator.Instance.Publish(new PlayerDied("Hero"));
```

Unlike `Sagittaras.Dices`' `DiceBag.Instance`, `Mediator.Instance` has no setter — it is a single fixed instance
for the lifetime of the process, not a slot you can swap out. If you need multiple isolated mediators (e.g. one
per game session, or a fake instance for a unit test), instantiate `Mediator` directly instead:

```csharp
IMediator mediator = new Mediator();
```

- **`Subscribe<TContract>`** registers a callback for a given contract type. Subscribing the same method twice
  is a no-op — a subscriber is identified by its delegate's hash code, so duplicate registrations are silently
  ignored rather than invoking the callback multiple times per publish.
- **`Unsubscribe<TContract>`** removes a previously registered callback. Unsubscribing a callback that was never
  registered is also a no-op.
- **`Publish<TContract>`** synchronously invokes every subscriber currently registered for `TContract`, in
  registration order. There is no queuing or deferred delivery — the call returns once all subscribers have run.

### Exception Handling

A throwing subscriber does not stop delivery to the remaining subscribers, nor does it propagate out of
`Publish`. Each subscriber is invoked inside its own `try/catch`; any exception is wrapped in a
`SubscriberException` and surfaced through the static `Mediator.ExceptionRaised` event instead.

```csharp
Mediator.ExceptionRaised += (sender, e) =>
{
    Debug.LogError($"Mediator subscriber failed: {e.Message}");
    // e.Contract    - the IMediatorContract instance being published
    // e.Subscriber  - the IMediatorSubscriber that threw
    // e.InnerException - the original exception
};
```

`ExceptionRaised` is `static`, so subscribing to it once (e.g. at application startup) captures failures from
every `Mediator` instance — there is no built-in logging otherwise, since the package has no runtime dependency
to log through.

## Usage Notes

- **Subscriber identity is the delegate, not the method group.** Two `Action<TContract>` instances created from
  the same instance method are equal (and therefore deduplicated / matched on `Unsubscribe`) because delegate
  equality is based on target and method, not on object identity of the delegate wrapper.
- **Delivery is synchronous.** If a subscriber is expensive or triggers further `Publish` calls, be mindful of
  re-entrancy and frame-time cost — the mediator does not defer or batch dispatch on your behalf.
- **Contracts are looked up by exact runtime type.** Subscribing to a base contract type does not receive
  publishes of a derived contract type, and vice versa — `Publish<TContract>` only reaches subscribers
  registered for that exact `TContract`.

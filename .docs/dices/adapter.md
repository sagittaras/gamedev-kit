# Adapter

`IDiceBagAdapter` is the RNG backend used by `DiceBag`. Swapping the adapter lets you change how random
numbers are generated — whether to use a different algorithm, seed strategy, or a deterministic
implementation for testing.

## Default Adapter

By default, `DiceBag` uses `SystemRandomAdapter`, which wraps `System.Random` seeded from `Guid.NewGuid()`:

```csharp
// These are equivalent
IDiceBag dice = new DiceBag();
IDiceBag dice = new DiceBag(IDiceBagAdapter.Default);
```

## Injecting a Custom Adapter

Pass any `IDiceBagAdapter` implementation to the `DiceBag` constructor:

```csharp
IDiceBagAdapter myAdapter = new MyCustomAdapter();
IDiceBag dice = new DiceBag(myAdapter);
```

## Implementing IDiceBagAdapter

The interface has two members:

```csharp
public interface IDiceBagAdapter
{
    // Returns a random integer in [min, max) — max is exclusive
    int Next(int min, int max);

    // Re-initializes the internal RNG state
    void Reseed();
}
```

A minimal custom implementation:

```csharp
public class SeededAdapter : IDiceBagAdapter
{
    private Random _random;
    private readonly int _seed;

    public SeededAdapter(int seed)
    {
        _seed = seed;
        _random = new Random(seed);
    }

    public int Next(int min, int max) => _random.Next(min, max);

    public void Reseed() => _random = new Random(_seed);
}
```

## Deterministic Adapter for Unit Tests

Random outcomes make tests flaky. A deterministic adapter returns a fixed sequence of values, so you can
assert exact results regardless of RNG state:

```csharp
public class SequenceAdapter : IDiceBagAdapter
{
    private readonly Queue<int> _values;

    public SequenceAdapter(params int[] values)
    {
        _values = new Queue<int>(values);
    }

    public int Next(int min, int max)
    {
        // Return next value from the queue, clamped to [min, max)
        int value = _values.Dequeue();
        return Math.Clamp(value, min, max - 1);
    }

    public void Reseed() { }
}
```

Usage in a test:

```csharp
[Test]
public void Damage_AlwaysRollsMaximum()
{
    // d8 rolls sides 1–8; adapter always returns 8
    var adapter = new SequenceAdapter(8);
    IDiceBag dice = new DiceBag(adapter);

    DieRoll damage = new DieRoll(5, 8); // 6–13
    int result = dice.Roll(damage);

    Assert.AreEqual(13, result); // 5 base + 8
}
```

This pattern also works for `ChanceTable<T>` tests — inject the adapter through the `IDiceBag`
parameter of the `ChanceTable` constructor to control which entry gets selected.

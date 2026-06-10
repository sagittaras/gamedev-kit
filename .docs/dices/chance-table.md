# ChanceTable\<T\>

`ChanceTable<T>` is a weighted probability table for randomly selecting an entry from a set of options,
where each option has a different likelihood of being chosen. Common use cases include loot tables,
procedural event selection, and enemy spawn weighting.

## Building a Table

The table is constructed from a collection of `KeyValuePair<T, Chance>` entries:

```csharp
var table = new ChanceTable<string>(new Dictionary<string, Chance>
{
    { "Common",    new Chance(60f) },  // 60%
    { "Rare",      new Chance(30f) },  // 30%
    { "Legendary", new Chance(10f) },  // 10%
});
```

An optional `IDiceBag` instance can be passed as the second constructor argument. If omitted,
`DiceBag.Instance` is used.

```csharp
var table = new ChanceTable<string>(entries, myDiceBag);
```

## Selecting an Entry

```csharp
if (table.TryNext(out string? loot))
{
    Debug.Log($"Dropped: {loot}");
}
```

`TryNext` returns `true` and sets the output parameter when an entry is selected.
It returns `false` when the roll falls into the "no chance" bucket (see below).

## Probabilities Below 100%

If the total of all chances is less than 100%, the remaining probability becomes a "no result" bucket.
`TryNext` returns `false` for rolls that land in this bucket.

```csharp
// Total: 70% — 30% chance of no result
var table = new ChanceTable<string>(new Dictionary<string, Chance>
{
    { "Sword",  new Chance(40f) },
    { "Shield", new Chance(30f) },
});

if (!table.TryNext(out string? drop))
{
    // 30% of the time: nothing dropped
}
```

## Probabilities Above 100%

If the total exceeds 100%, all entries are automatically normalized proportionally so they fit within
the full range. No entry is dropped — the relative weights between them are preserved.

```csharp
// Total: 150% — automatically normalized to 100%
var table = new ChanceTable<string>(new Dictionary<string, Chance>
{
    { "Common",    new Chance(90f) },  // → ~60%
    { "Rare",      new Chance(45f) },  // → ~30%
    { "Legendary", new Chance(15f) },  // → ~10%
});
```

## Use Case: Loot Table

```csharp
public class LootTable
{
    private readonly ChanceTable<ItemId> _table;

    public LootTable(IDiceBag diceBag)
    {
        _table = new ChanceTable<ItemId>(new Dictionary<ItemId, Chance>
        {
            { ItemId.HealthPotion, new Chance(50f) },
            { ItemId.ManaPotion,   new Chance(30f) },
            { ItemId.GoldCoin,     new Chance(15f) },
            { ItemId.RareGem,      new Chance(5f)  },
        }, diceBag);
    }

    public bool TryDrop(out ItemId item) => _table.TryNext(out item);
}
```

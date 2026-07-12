# Experience

`Experience` is a readonly value type wrapping a single `int` — the amount of experience points gained,
required, or remaining. It exists so experience amounts carry their own meaning through the API instead of
being passed around as bare `int`s, while still converting to and from `int` implicitly wherever that's
convenient.

## Creating

```csharp
Experience xp = new Experience(150);

// Implicit conversion from int.
Experience fromInt = 150;

// A zero experience value, useful as an accumulator starting point.
Experience zero = Experience.Zero;
```

## Conversion

`Experience` converts implicitly to and from `int`, so it composes with plain arithmetic and existing `int`-based
code without explicit casts:

```csharp
Experience xp = 100;
int raw = xp; // implicit conversion to int
```

## Arithmetic

```csharp
Experience total = new Experience(100) + new Experience(50); // 150
Experience total2 = new Experience(100) + 50;                // 150 - int on the right-hand side works too
Experience remaining = total - 30;                            // 120
```

## Comparison

`Experience` implements `IComparable<Experience>` and `IEquatable<Experience>`, plus the full set of comparison
operators (`==`, `!=`, `<`, `<=`, `>`, `>=`), all comparing by `Value`:

```csharp
if (currentXp >= requiredXp)
{
    // threshold reached
}
```

> This is what powers `Progress.Reached` — see [Progress](./progress.md).

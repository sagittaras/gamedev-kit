# Sagittaras.Dices

`Sagittaras.Dices` is a dice rolling and randomness library for game logic. It provides typed abstractions over
random number generation — rather than calling `Random.Next()` directly, you express intent through value types
like `DieRoll`, `Chance`, and `RandomRange`, and let `DiceBag` resolve them. The goal is semantic clarity:
each type carries its own meaning and constraints, so the intent behind a random operation is visible directly
in the code rather than buried in magic numbers.

## Dependencies

| Package | Required |
|---|---|
| [Sagittaras.GuardClauses](../guard-clauses/index.md) | ✅ |

Make sure `Sagittaras.GuardClauses.dll` is present in `Assets/Plugins/` alongside `Sagittaras.Dices.dll`.

## Architecture

The library is built around three layers:

```
IDiceBag          ← your entry point for all randomness operations
    └── IDiceBagAdapter   ← swappable RNG backend (default: System.Random)
```

**`DiceBag`** is the default implementation of `IDiceBag`. A global singleton (`DiceBag.Instance`) is available
for convenience, but you can also instantiate it directly and inject a custom adapter — useful for testing.

## Getting Started

```csharp
// Use the global instance
IDiceBag dice = DiceBag.Instance;

// Roll a d6 (base 0, 6 sides → result: 0–6)
int roll = dice.Roll(new DieRoll(0, 6));

// Check a 25% chance
bool success = dice.Try(new Chance(25f));

// Random integer in range [1, 100]
int value = dice.Next(new RandomRange(1, 100));
```

## Types

| Page | Description |
|---|---|
| [Chance](./chance.md) | Probability value type with operators and extension methods |
| [DieRoll](./die-roll.md) | Die configuration for multi-sided dice rolls |
| [RandomRange](./random-range.md) | Inclusive integer range for bounded random values |
| [ChanceTable\<T\>](./chance-table.md) | Weighted probability table for loot and procedural selection |
| [Adapter](./adapter.md) | Custom RNG backends and testing with deterministic adapters |

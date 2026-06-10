# Sagittaras Game Development Kit

**Game Development Kit** is a collection of open-source C# libraries by [Sagittaras Games](https://sagittaras.games),
built and battle-tested during the development of our own titles — *Spellborn* and *Vectro Blast*. We open-source
these solutions so the indie community can benefit from the same tools we rely on, and to demonstrate the quality of
our engineering.

## Installation

Each package is distributed as a compiled DLL via **[GitHub Releases](https://github.com/sagittaras/gamedev-kit/releases)**.
To add a package to your Unity project:

1. Download the desired `.dll` from the latest release.
2. Place it in your project under `Assets/Plugins/`.
3. Unity will automatically detect and reference the assembly.

> If you use multiple packages, place all DLLs in the same `Assets/Plugins/` folder.

> **Check dependencies.** Some packages require other packages from this kit to function. Before importing
> a package, review its dependencies listed in the documentation and include all required DLLs.

---

## Packages

### Sagittaras.Dices

Dice rolling and randomness abstractions for game logic. Provides a flexible `IDiceBag` interface with a swappable
RNG adapter, `DieRoll` value type for configuring multi-sided dice, `Chance`-based probability evaluation,
and a weighted `ChanceTable<T>` for loot tables and procedural selection. Extension methods on both `IDiceBag`
and `Chance` cover common patterns like coin flips and weighted picks.

```csharp
IDiceBag dice = DiceBag.Instance;

// Roll a d20 (base 0, 20 sides)
int result = dice.Roll(new DieRoll(0, 20));

// Evaluate a 30% chance — Chance accepts float (0.0–100.0) or int (0–10000)
bool success = dice.Try(new Chance(30f));

// Coin flip between two values
string side = dice.FlipCoin("Heads", "Tails");

// Weighted loot table
var table = new ChanceTable<string>(new Dictionary<string, Chance>
{
    { "Common",    new Chance(60f) },
    { "Rare",      new Chance(30f) },
    { "Legendary", new Chance(10f) },
});

if (table.TryNext(out string? loot))
{
    Debug.Log($"Dropped: {loot}");
}
```

> 📦 [Documentation](./dices/index.md) · **Dependencies:** [Sagittaras.GuardClauses](#sagittarasguardclauses)

---

### Sagittaras.GuardClauses

Lightweight guard clause pattern for defensive programming. Provides a fluent `Guard.Against` entry point
with extension methods for common input validation — range checks, comparisons, and zero guards — that throw
descriptive `ArgumentOutOfRangeException` instead of silent failures.

```csharp
public void SetHealth(int value, int max)
{
    Guard.Against.LessThan(value, 0);       // value must be >= 0
    Guard.Against.GreaterThan(value, max);  // value must be <= max

    _health = value;
}
```

> 📦 [Documentation](./guard-clauses/index.md) · **Dependencies:** none

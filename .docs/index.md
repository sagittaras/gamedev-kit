<h1 align="center">Sagittaras Game Development Kit</h1>

<p align="center">
    <a href="./guard-clauses/index.md"><img src="https://img.shields.io/badge/Sagittaras.GuardClauses-darkgreen?style=flat-square" alt="Sagittaras.GuardClauses"/></a>
    <a href="./dices/index.md"><img src="https://img.shields.io/badge/Sagittaras.Dices-blueviolet?style=flat-square" alt="Sagittaras.Dices"/></a>
    <a href="./timing/index.md"><img src="https://img.shields.io/badge/Sagittaras.Timing-blue?style=flat-square" alt="Sagittaras.Timing"/></a>
    <a href="./messaging/index.md"><img src="https://img.shields.io/badge/Sagittaras.Messaging-crimson?style=flat-square" alt="Sagittaras.Messaging"/></a>
    <a href="./progression/index.md"><img src="https://img.shields.io/badge/Sagittaras.Progression-orange?style=flat-square" alt="Sagittaras.Progression"/></a>
</p>

**Game Development Kit** is a collection of open-source C# libraries by [Sagittaras Games](https://sagittaras.games),
built and battle-tested during the development of our own titles — *Spellborn* and *Vectro Blast*. We open-source
these solutions so the indie community can benefit from the same tools we rely on, and to demonstrate the quality of
our engineering.

## Installation

### Package Manager

Every package is a signed Unity package on [OpenUPM](https://openupm.com), named `com.sagittaras.gamedevkit.<package>`
(see the [README](../README.md#getting-started) for the list) and requiring Unity 2021.3 or newer. The Package Manager
installs the dependencies between the packages for you. Add a package in one of two ways.

**With [openupm-cli](https://github.com/openupm/openupm-cli)** — run in your Unity project's folder:

```bash
openupm add com.sagittaras.gamedevkit.<package>
```

**With a scoped registry** — add it once in **Project Settings → Package Manager → Scoped Registries**:

| Field | Value |
| --- | --- |
| Name | `Sagittaras Games` |
| URL | `https://package.openupm.com` |
| Scope(s) | `com.sagittaras.gamedevkit` |

Then install the packages from **My Registries** in the Package Manager window.

### Without the Package Manager

Every version also has its **[GitHub Release](https://github.com/sagittaras/gamedev-kit/releases)** with a zip archive
of the compiled assemblies, including the DLLs of the kit packages it depends on. Reference them from a plain .NET
project, or extract the archive under `Assets/Plugins/` in Unity — but never install the same package through OpenUPM
and `Assets/Plugins/` at once, or Unity reports duplicate assemblies.

#### `.pdb` and `.xml` files

Alongside every `.dll`, the archive also carries a matching `.pdb` and `.xml` file (the Unity package ships them too,
with no extra step). Neither is required for the package to work, but both are worth keeping, especially if you're
coming from Unity's own C# scripting and haven't worked much with precompiled .NET assemblies before:

- **`.xml`** is the assembly's documentation file. It carries the same `<summary>` descriptions you'd see in
  this documentation, but surfaced directly in your IDE — hover over any type or method from the package (e.g.
  in Rider or Visual Studio) and you get IntelliSense tooltips, instead of having to jump back here or into
  decompiled code to see what something does.
- **`.pdb`** carries debug symbols. Without it, an exception thrown inside the package shows up in stack traces
  as an unhelpful reference to the assembly with no file or line number. With the matching `.pdb` next to the
  `.dll`, stack traces resolve to the actual file and line, which makes bug reports (and your own debugging)
  far more useful.

Drop both files in the same `Assets/Plugins/` folder as the `.dll` — Unity picks them up automatically, no extra
configuration needed. The `.pdb` also carries Source Link, so Rider or Visual Studio can step
into the package's source straight from GitHub.

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

> 📦 [Documentation](./dices/index.md) · **Unity:** [`com.sagittaras.gamedevkit.dices`](https://openupm.com/packages/com.sagittaras.gamedevkit.dices/) · **Dependencies:** [Sagittaras.GuardClauses](#sagittarasguardclauses)

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

> 📦 [Documentation](./guard-clauses/index.md) · **Unity:** [`com.sagittaras.gamedevkit.guard-clauses`](https://openupm.com/packages/com.sagittaras.gamedevkit.guard-clauses/) · **Dependencies:** none

---

### Sagittaras.Timing

Delta-time based timer primitives for game loops. Provides `IntervalTimer` for recurring interval tracking,
`CallbackTimer` for registering actions that fire at a set rate, and `Cooldown` for single-shot duration
guards that wait for an explicit reset — all driven by manual `deltaTime` accumulation.

```csharp
// Fire logic every 2 seconds
var timer = new IntervalTimer(2f);

// Invoke a callback every 0.5 s
var spawner = new CallbackTimer(0.5f, () => SpawnEnemy());

// Global cooldown — 500 ms, controlled reset
var gcd = new Cooldown(0.5f);
if (gcd.IsReady) 
{ 
    ExecuteAction(); 
    gcd.Reset(); 
}
```

> 📦 [Documentation](./timing/index.md) · **Unity:** [`com.sagittaras.gamedevkit.timing`](https://openupm.com/packages/com.sagittaras.gamedevkit.timing/) · **Dependencies:** [Sagittaras.GuardClauses](#sagittarasguardclauses)

---

### Sagittaras.Messaging

Mediator / Pub-Sub library for loosely coupled communication between game components. Components exchange
plain `IMediatorContract` messages through a central `IMediator`, so publishers and subscribers never need
direct references to each other. Delivery is synchronous, duplicate subscriptions are deduplicated by
delegate identity, and subscriber exceptions are caught and surfaced through a static `ExceptionRaised`
event instead of interrupting the publish.

```csharp
public record PlayerDied(string PlayerName) : IMediatorContract;

IMediator mediator = Mediator.Instance;
mediator.Subscribe<PlayerDied>(OnPlayerDied);

mediator.Publish(new PlayerDied("Hero"));

void OnPlayerDied(PlayerDied contract)
{
    Debug.Log($"{contract.PlayerName} has died.");
}
```

> 📦 [Documentation](./messaging/index.md) · **Unity:** [`com.sagittaras.gamedevkit.messaging`](https://openupm.com/packages/com.sagittaras.gamedevkit.messaging/) · **Dependencies:** none

---

### Sagittaras.Progression

Level and experience progression for games. Provides `Experience` and `Progress` value types for tracking
experience towards the next level, a `Level` type exposing `Gain`/`LevelUp` for resolving level-ups (including
multiple at once), and a swappable `IExperienceFormula` — defaulting to an exponential curve — for defining how
much experience each level requires.

```csharp
Level level = new Level(1);

LevelGainResult result = level.Gain(new Experience(1000));
if (result.LeveledUp)
{
    Debug.Log($"Gained {result.LevelsGained} level(s)!");
}

level = result;
```

> 📦 [Documentation](./progression/index.md) · **Unity:** [`com.sagittaras.gamedevkit.progression`](https://openupm.com/packages/com.sagittaras.gamedevkit.progression/) · **Dependencies:** [Sagittaras.GuardClauses](#sagittarasguardclauses)

---

## FAQ
See [FAQ](./faq.md)
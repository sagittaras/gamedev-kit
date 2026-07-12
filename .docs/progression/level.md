# Level

`Level` is the main entry point of the package — it carries the current level value, its
[`Progress`](./progress.md) towards the next level, and the [`IExperienceFormula`](./formulas.md) used to
compute thresholds. `Level` is immutable: every operation that changes state returns a new `Level` instance.

## Creating

```csharp
// Level 1, using the default formula (ExponentialScalingFormula).
Level level = new Level(1);

// A shared constant for the default starting level.
Level starting = Level.MinValue; // level 1

// A custom formula and/or restored overflow experience (e.g. loading a save).
Level restored = new Level(3, new ExponentialScalingFormula(baseValue: 200), overflow: 40);
```

The constructor guards against levels below `1` (`Guard.Against.LessThan`), throwing
`ArgumentOutOfRangeException` for anything lower.

## Members

```csharp
public readonly partial struct Level
{
    public int Value { get; }
    public Progress Progress { get; }

    public LevelGainResult Gain(Experience experience);
    public Level LevelUp();
}
```

- **`Value`** — the current level number.
- **`Progress`** — experience accumulated towards the next level, and the threshold required (see
  [Progress](./progress.md)).
- **`Level` converts implicitly to `int`**, mirroring `Value`, so it drops into existing `int`-based level
  checks without a cast:

```csharp
int levelNumber = level; // implicit conversion
```

## Adding Experience with `Gain`

`Gain` is the high-level way to award experience — it resolves as many level-ups as the amount allows in a
single call, carrying overflow experience forward into each new level:

```csharp
Level level = new Level(1);
LevelGainResult result = level.Gain(new Experience(1000)); // may span multiple levels

result.LevelsGained; // number of levels gained, 0 if none
result.LeveledUp;    // true when LevelsGained > 0

Level current = result; // LevelGainResult converts implicitly to Level
```

## Manual Progress and `LevelUp`

For finer control, add experience to `Progress` directly via the `+`/`-` operators, then call `LevelUp` once
the threshold is reached:

```csharp
Level level = new Level(1);
level += new Experience(100); // only adjusts Progress.Value, does not level up by itself

if (level.Progress.Reached)
{
    level = level.LevelUp();
}
```

`LevelUp` guards that the threshold has actually been reached (`Guard.Against.False`), throwing
`InvalidOperationException` otherwise — so calling it speculatively without checking `Progress.Reached` first
is a programming error, not an expected flow:

```csharp
Level level = Level.MinValue;
level.LevelUp(); // throws InvalidOperationException - threshold not reached yet
```

`LevelUp` also carries over any XP earned beyond the threshold as overflow into the new level's `Progress`,
so no experience is lost when a level-up isn't exact.

## `LevelGainResult`

The return value of `Gain`, describing what happened during the operation:

```csharp
public readonly struct LevelGainResult
{
    public Level Level { get; }
    public int LevelsGained { get; }
    public bool LeveledUp { get; }
}
```

`LevelGainResult` converts implicitly to `Level`, so you can usually assign the result straight back to your
current level variable without unwrapping `.Level` explicitly:

```csharp
level = level.Gain(xpReward);
```

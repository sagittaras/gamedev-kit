# Formulas

`IExperienceFormula` decides how much [`Experience`](./experience.md) is required to advance from a given level
to the next one. It is the pluggable piece of the package — swap it to change the progression curve without
touching `Level` itself.

```csharp
public interface IExperienceFormula
{
    static IExperienceFormula Default = new ExponentialScalingFormula();

    Experience Calculate(int currentLevel);
}
```

`Calculate` receives the *current* level and returns the experience threshold to reach the *next* one — e.g.
`Calculate(1)` is the amount of experience needed to go from level 1 to level 2.

## Default Formula

By default, `Level` uses `IExperienceFormula.Default`, an `ExponentialScalingFormula`:

```csharp
// These are equivalent.
Level level = new Level(1);
Level level2 = new Level(1, IExperienceFormula.Default);
```

`ExponentialScalingFormula` computes the threshold as `baseValue * multiplier ^ (currentLevel - 1)`:

```csharp
var formula = new ExponentialScalingFormula(baseValue: 100, multiplier: 1.25f);

formula.Calculate(1); // 100
formula.Calculate(2); // 125
formula.Calculate(3); // 156
```

Both `baseValue` (default `100`) and `multiplier` (default `1.25f`) are constructor parameters, so you can tune
the curve's steepness without implementing your own formula:

```csharp
// A steeper curve for a shorter, more aggressive progression.
var steep = new ExponentialScalingFormula(baseValue: 200, multiplier: 1.5f);
Level level = new Level(1, steep);
```

## Implementing a Custom Formula

Implement `IExperienceFormula` directly for curves that don't fit an exponential shape — e.g. a flat rate, a
linear curve, or a designer-authored lookup table:

```csharp
public class LinearFormula : IExperienceFormula
{
    private readonly int _perLevel;

    public LinearFormula(int perLevel = 100)
    {
        _perLevel = perLevel;
    }

    public Experience Calculate(int currentLevel)
    {
        return currentLevel * _perLevel;
    }
}
```

```csharp
Level level = new Level(1, new LinearFormula(perLevel: 150));
```

Pass the formula to every `Level` constructed for a given entity — `Level` carries its formula internally and
reuses it on every subsequent level-up (via `LevelUp` / `Gain`), so it only needs to be provided once, at
creation or when restoring a save.

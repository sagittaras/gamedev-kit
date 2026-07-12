# Sagittaras.Progression

`Sagittaras.Progression` is a level and experience progression library for games. It provides value types for
tracking experience and level progress (`Experience`, `Progress`, `Level`), and a swappable `IExperienceFormula`
that decides how much experience is required to reach the next level — so the progression curve is a single
pluggable piece rather than something scattered across your gameplay code.

## Dependencies

| Package | Required |
|---|---|
| [Sagittaras.GuardClauses](../guard-clauses/index.md) | ✅ |

Make sure `Sagittaras.GuardClauses.dll` is present in `Assets/Plugins/` alongside `Sagittaras.Progression.dll`.

## Architecture

```
Level              ← current level + progress towards the next one
    └── Progress         ← current Experience vs. the Experience threshold
    └── IExperienceFormula ← swappable curve deciding the threshold per level
```

**`Level`** is the entry point. Each `Level` carries a `Progress` (current experience vs. threshold) and the
`IExperienceFormula` used to compute that threshold. Awarding experience is done through `Level.Gain`, which
returns a `LevelGainResult` describing whether — and how many times — the level was gained.

## Getting Started

```csharp
using Sagittaras.Progression;

// Start at level 1, using the default exponential formula.
Level level = new(1);

// Award experience gained during gameplay.
LevelGainResult result = level.Gain(new Experience(150));

if (result.LeveledUp)
{
    Debug.Log($"Leveled up {result.LevelsGained} time(s)! Now level {(int)result.Level}.");
}

// The result implicitly converts to Level - carry it forward as the new current level.
level = result;

Debug.Log($"{level.Progress.Value} / {level.Progress.Threshold} XP to next level.");
```

`Gain` awards experience and resolves as many level-ups as the amount allows in one call — including multiple
levels at once — carrying any overflow experience into the new level's progress.

## Types

| Page | Description |
|---|---|
| [Experience](./experience.md) | Value type representing an amount of experience points |
| [Progress](./progress.md) | Current experience against the threshold for the next level |
| [Level](./level.md) | Current level, its progress, and level-up operations |
| [Formulas](./formulas.md) | Pluggable experience curve via `IExperienceFormula` |

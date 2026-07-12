# Progress

`Progress` tracks a current [`Experience`](./experience.md) value against the `Experience` threshold required
to reach the next level. It is the piece of `Level` that actually accumulates experience — `Level` itself is
mostly a wrapper that knows *which* level it is and *which formula* produced the threshold.

## Creating

```csharp
// Threshold-only - starts at zero experience.
Progress progress = new Progress(threshold: 250);

// Explicit starting value, e.g. when restoring saved progress.
Progress restored = new Progress(threshold: 250, value: 80);
```

In practice you rarely construct `Progress` directly — `Level` creates and manages it for you via its
`IExperienceFormula` (see [Formulas](./formulas.md)).

## Members

```csharp
public readonly struct Progress
{
    public Experience Value { get; }
    public Experience Threshold { get; }
    public bool Reached { get; }
}
```

- **`Value`** — experience accumulated so far towards the next level.
- **`Threshold`** — experience required to reach the next level.
- **`Reached`** — `true` once `Value >= Threshold`.

## Adding and Removing Experience

```csharp
Progress progress = new Progress(threshold: 250);

progress += 100; // Value: 100 / 250
progress += 200; // Value: 300 / 250 - Reached is now true

progress -= 50;  // Value: 250 / 250 - still Reached
```

Note that adding experience via `+`/`-` never changes `Threshold` and never rolls over into a new level on its
own — it only moves `Value`. Resolving a reached threshold into an actual level-up is the job of
[`Level.Gain` and `Level.LevelUp`](./level.md).

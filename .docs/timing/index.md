# Sagittaras.Timing

`Sagittaras.Timing` is a delta-time based timing library for game loops. It provides lightweight timer
primitives that accumulate time across frames. Reducing the _cooldown boilerplate_ in the code.

## Dependencies

| Package | Required |
|---|---|
| [Sagittaras.GuardClauses](../guard-clauses/index.md) | ✅ |

Make sure `Sagittaras.GuardClauses.dll` is present in `Assets/Plugins/` alongside `Sagittaras.Timing.dll`.

## Getting Started

All timers are updated manually by passing `deltaTime` on each frame. There is no background thread or
Unity coroutine involved — you control when and how often each timer advances.

```csharp
// IntervalTimer — fires every 2 seconds
var interval = new IntervalTimer(2f);

// CallbackTimer — invokes the registered action every 0.5 s
var callback = new CallbackTimer(0.5f, DoSomething);

// Cooldown — tracks whether the cooldown period has passed
var cooldown = new Cooldown(1f);

void Update()
{
    float delta = Time.deltaTime;
    
    // Internval Timer
    if (interval.TryTick(delta))
    {
        Debug.Log("Interval elapsed.");
    }
    
    // Callback Timer
    callback.TryUse(delta);
    
    // Cooldown
    cooldown.Update(delta);
    if (cooldown.IsReady)
    {
        Debug.Log("Action is available.");
        cooldown.Reset(); // Reset the ready state when needed.
    }
}

void DoSomething() 
{
    Debug.Log("Called by Callback Timer every 0.5 s.");
}
```

## Types

### `IntervalTimer`

The foundational timer primitive. Accumulates delta time across `TryTick` calls and returns `true` once
the configured interval elapses, then resets automatically for the next cycle. Use this when you need
raw interval tracking.

```csharp
var timer = new IntervalTimer(2f); // fires every 2 seconds

void Update()
{
    if (timer.TryTick(Time.deltaTime))
    {
        // runs once every 2 seconds
    }
}
```

Pass `startImmediately: true` to make the first `TryTick` call return `true` without waiting for the
full interval to elapse.

---

### `CallbackTimer`

Wraps `IntervalTimer` and invokes a registered `Action` each time the interval elapses. Prefer this over
`IntervalTimer` when the response to the timer is always the same action and you want to keep the update
loop clean.

```csharp
var timer = new CallbackTimer(0.5f, SpawnEnemy);

void Update()
{
    timer.TryUse(Time.deltaTime);
}

void SpawnEnemy()
{
    // Your spawning logic.
}
```

---

### `Cooldown`

Tracks a single cooldown period. Unlike `IntervalTimer`, it does not reset automatically — once the
duration elapses and `IsReady` returns `true`, it stays ready until explicitly reset. Use this for
abilities, global cooldowns, or any mechanic where the player (or game logic) controls when the next
cycle begins.

```csharp
var gcd = new Cooldown(0.5f); // 500 ms global cooldown

void OnPlayerAction()
{
    if (!gcd.IsReady) return;

    ExecuteAction();
    gcd.Reset(); // restart the cooldown
}

void Update()
{
    gcd.Update(Time.deltaTime);
}
```

To prevent the cooldown to be ready immediately, pass `startsOnCooldown: true`.
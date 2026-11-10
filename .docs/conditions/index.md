# Sagittaras.Conditions

`Sagittaras.Conditions` is a general-purpose condition system for validating the state of game entities. A
**condition** is plain data describing a requirement — "caster is at least level 5", "target has no shield" —
and the package evaluates it against whatever entity your game puts in front of it.

Because conditions are data rather than code, they can live in your content files, be authored by designers,
and be shipped with items, spells or quests. The logic that actually performs a check is registered once, in
code, and reused by every condition of that type.

## Dependencies

This package has no dependencies on other packages from the kit.

## Architecture

```
ICondition                         ← plain data: what to check, on whom, with what parameters
    ├── ConditionType                  ← identifies the check   → IConditionHandler
    └── ConditionTarget                ← identifies the subject → ITargetResolver

IConditionEvaluator<TContext>      ← evaluates conditions within a single context
    ├── ITargetResolver<TContext, TSubject>  ← picks the subject out of the context
    └── IConditionHandler<TSubject>          ← performs the check itself

IConditionManager                  ← combines evaluators of different contexts
```

Evaluation always happens against a **context** — the situation being validated, such as a spell cast with its
caster and its target. The condition's `Target` says which entity from that context (the **subject**) the check
applies to, and the condition's `ConditionType` says which check to run.

## Getting Started

```csharp
using Sagittaras.Conditions;
using Sagittaras.Conditions.Evaluation;
using Sagittaras.Conditions.Handlers;

// 1. Declare the condition types and targets your game understands.
public static class GameConditions
{
    public static readonly ConditionType Level = new("game.level");
    public static readonly ConditionTarget Caster = new("game.caster");
    public static readonly ConditionTarget Target = new("game.target");
}

// 2. Describe the situation being validated.
public record CastContext(IUnit Caster, IUnit? Target);

// 3. Implement the check. ComparisonHandler applies the condition's Comparison for you.
public class LevelHandler : ComparisonHandler<IUnit>
{
    protected override int GetActualValue(ICondition condition, IUnit subject) => subject.Level;
}

// 4. Wire it together once, at startup.
IConditionEvaluator<CastContext> evaluator = new ConditionEvaluatorBuilder<CastContext, IUnit>()
    .WithHandler(GameConditions.Level, new LevelHandler())
    .WithResolver(GameConditions.Caster, context => context.Caster)
    .WithResolver(GameConditions.Target, context => context.Target)
    .Build();

// 5. Evaluate whenever the game needs to know.
if (evaluator.Evaluate(spell, new CastContext(caster, target)))
{
    Cast(spell);
}
```

`spell` here is any `IConditional` — an entity carrying a list of conditions. All of them must be satisfied for
the conditional to pass.

## Types

### `ICondition` / `IConditional`

`ICondition` is the data of a single requirement. Implement it on whatever your content pipeline produces —
a record, a serializable class, a Unity `ScriptableObject`:

```csharp
public interface ICondition
{
    ConditionType ConditionType { get; }   // which check to run
    ConditionTarget Target { get; }        // whose state to check
    IReadOnlyList<int> Parameters { get; } // values the check needs
    bool Negated { get; }                  // invert the result
    Comparison Comparison { get; }         // operator for single-parameter checks
}
```

`IConditional` marks an entity that carries conditions — a spell, an item, a quest step:

```csharp
public interface IConditional
{
    IReadOnlyList<ICondition> Conditions { get; }
}
```

### `ConditionType` / `ConditionTarget`

Both are read-only structs wrapping a string identifier, used as **open enums**. Unlike a C# `enum`, which is
closed to the assembly that declares it, new values can be added by any assembly — a game module, a DLC, a mod:

```csharp
public static readonly ConditionType HasAura = new("game.has-aura");
```

Identifiers are compared by ordinal string equality, so prefixing them per domain (`game.`, `quest.`) keeps
values from separate systems apart. Both structs are safe to use as dictionary keys, and their default value
does not throw — it simply matches nothing.

### `IConditionHandler` / `ComparisonHandler`

A handler performs one kind of check against a subject:

```csharp
public interface IConditionHandler<in TSubject>
{
    bool Evaluate(ICondition condition, TSubject subject);
}
```

Handlers do not deal with negation — the evaluator applies `Negated` to whatever they return.

`ComparisonHandler<TSubject>` is a base class for the common case of comparing a value of the subject against a
value from the condition, using its `Comparison` operator. Derived handlers only provide the subject's value:

```csharp
public class HealthHandler : ComparisonHandler<IUnit>
{
    protected override int GetActualValue(ICondition condition, IUnit subject) => subject.Health;
}
```

The expected value defaults to `Parameters[0]`. For conditions carrying more parameters — an item id followed by
a required amount — override `GetExpectedValue`.

### `ITargetResolver`

A resolver picks the subject for one `ConditionTarget` out of the context:

```csharp
public interface ITargetResolver<in TContext, TSubject>
{
    bool TryResolve(TContext context, out TSubject subject);
}
```

Returning `false` means the context has no such subject right now — a spell cast without a target, a target that
died mid-cast. That is a normal game state, not an error. Simple resolvers can be registered as a function
returning `null` instead of implementing the interface.

### `IConditionEvaluator` / `ConditionEvaluator`

The evaluator ties handlers and resolvers together. It resolves the subject, runs the handler, applies negation,
and for an `IConditional` stops at the first unsatisfied condition.

Evaluators are created through `ConditionEvaluatorBuilder<TContext, TSubject>` and are immutable afterwards, so a
built evaluator can be shared across systems. Registrations can be composed from several places — each module can
contribute its own conditions through an extension method:

```csharp
public static ConditionEvaluatorBuilder<CastContext, IUnit> AddCombatConditions(
    this ConditionEvaluatorBuilder<CastContext, IUnit> builder)
{
    return builder
        .WithHandler(GameConditions.Level, new LevelHandler())
        .WithHandler(GameConditions.Health, new HealthHandler());
}
```

Registering the same condition type or target twice throws `ArgumentException` at the registering call.

### `IConditionManager` / `ConditionManager`

Larger games validate more than one kind of entity — spells against a cast context, quests against a quest
context. Each needs its own evaluator, and the manager combines them into a single entry point:

```csharp
IConditionManager manager = new ConditionManagerBuilder()
    .WithEvaluator(castEvaluator)   // IConditionEvaluator<CastContext>
    .WithEvaluator(questEvaluator)  // IConditionEvaluator<QuestContext>
    .Build();

if (manager.Evaluate(spell, castContext)) { /* ... */ }
if (manager.Evaluate(quest, questContext)) { /* ... */ }
```

The manager has no global instance — hold it wherever your game keeps its services, typically a dependency
injection container.

A system evaluating conditions repeatedly can take its evaluator out once instead of looking it up per call:

```csharp
if (manager.TryGetEvaluator(out IConditionEvaluator<CastContext>? evaluator))
{
    foreach (ISpell spell in spells)
    {
        spell.IsAvailable = evaluator.Evaluate(spell, castContext);
    }
}
```

## Usage Notes

- **The manager dispatches by the context type at the call site.** `TContext` is inferred from the declared type
  of the argument, not from the object's runtime type, so a context passed as its base type or interface will not
  find an evaluator registered for the derived type. Keep contexts concrete, or pass the type explicitly.
- **An unresolved subject fails the condition, even a negated one.** The condition could not be evaluated at all,
  so it is not satisfied — "target does not have a shield" is not true when there is no target.
- **Missing registrations throw, missing subjects do not.** A condition type without a handler, a target without
  a resolver, or a context without an evaluator raise dedicated exceptions naming the missing key, because they
  are errors in data or setup rather than in game state.
- **Build once, evaluate many times.** Evaluators and managers are immutable after `Build()` and allocate nothing
  during evaluation, so they are safe to build at startup and share for the lifetime of the game.

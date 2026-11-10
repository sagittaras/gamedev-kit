# Sagittaras.Scripting

`Sagittaras.Scripting` lets a core module open up chosen places in its behavior — **hook-points** — so that other
modules can alter what happens there, without the core ever referencing them. It is the idea behind scripting in
WoW emulators, generalized: the core says *"here is where a creature decides whom to attack"*, and a separate
module answers for one particular creature.

That matters once a project is split into domain-specific packages. The combat package should not know that the
boss in the third dungeon fights differently, and it should not have to be rebuilt when a designer decides that it
does. With hook-points the dependency only ever points one way — the extending module knows the core, the core
knows nothing.

## Installation

| | |
|---|---|
| Namespace | `Sagittaras.Scripting` |
| Unity package ([OpenUPM](https://openupm.com/packages/com.sagittaras.gamedevkit.scripting/)) | `com.sagittaras.gamedevkit.scripting` |
| NuGet package ([nuget.org](https://www.nuget.org/packages/Sagittaras.Scripting/)) | `Sagittaras.Scripting` |

See [Installation](../index.md#installation) for adding the package to your project.

## Dependencies

This package has no dependencies on other packages from the kit.

## Architecture

```
[GameScript]                       ← marks an abstract base script as an entrypoint
    └── the hook-points a core module exposes, as virtual On{Event} members

IGameScript                        ← marker of every script
    ├── IGlobalScript                  ← no entity target       → one call reaches every script
    ├── IEntityScript                  ← target by entity id    → one shared instance per entity
    └── ITransientEntityScript<TArgs>  ← target by instance     → one instance per living entity

IScriptRegistryBuilder             ← collects the scripts while the game boots
    └── IScriptManager                 ← reaches the hook-points from the game loop

ScriptDiscovery                    ← finds the scripts a module assembly holds
IScriptActivator                   ← provides and recycles the transient instances
```

Entrypoints form a **hierarchy**: an object script, a world object script derived from it, a creature script
derived from that. A concrete script is reachable through every entrypoint above it, so the core can call a
hook-point at whichever level it is declared on and still reach the creature script that overrode it.

Each level has to opt in by `[GameScript]` of its own — the attribute is not inherited, so an intermediate class
that is merely shared implementation never becomes a call target.

The namespaces follow the same order of events: `Discovery` finds the scripts and `Building` registers them
while the game boots, `Registries` answers the lookups of the game loop, and `Activation` provides the transient
instances. `Entities` and `Attributes` hold what a game marks its own types with.

## Getting Started

### 1. The core module declares its hook-points

```csharp
using Sagittaras.Scripting;
using Sagittaras.Scripting.Attributes;
using Sagittaras.Scripting.Entities;

// What the core offers a creature script when the creature spawns.
public readonly struct CreatureSpawn
{
    public CreatureSpawn(int level) => Level = level;
    public int Level { get; }
}

[GameScript]
public abstract class CreatureScript : ITransientEntityScript<CreatureSpawn>
{
    protected ITransientScriptedEntity Entity { get; private set; } = null!;
    protected int Level { get; private set; }

    public virtual void Initialize(ITransientScriptedEntity entity, in CreatureSpawn args)
    {
        Entity = entity;
        Level = args.Level;
    }

    // The hook-points themselves.
    public virtual void OnAggro() { }
    public virtual int OnDamageTaken(int damage) => damage;
}
```

Every hook-point is named after the **moment** it opens up — `OnAggro`, `OnDamageTaken`, `OnGoldLooted`.
Read top to bottom, the base script is then the list of moments an extending module can step into, and
that list is most of the documentation such a module needs.

A hook-point that returns a value asks a question at that moment rather than announcing it: the core hands
in what it would have used on its own, and the script either answers with something else or passes it
straight back. `OnDamageTaken` above is the core asking *"how much damage does this creature actually
take?"*, and a creature with no script answers exactly as the core would have.

### 2. The creature itself is a scripted entity

```csharp
public class Creature : ITransientScriptedEntity
{
    public string Id { get; init; }       // the kind of creature, from your content files
    public int InstanceId { get; init; }  // this one creature, unique within its Id

    public event EventHandler<ITransientScriptedEntity>? Released;

    // Raise it when the creature dies or returns to its pool.
    public void Despawn() => Released?.Invoke(this, this);
}
```

### 3. An extending module writes the script

```csharp
using Sagittaras.Scripting.Attributes;

[ScriptedEntity("boss_onyxia")]
public class OnyxiaScript : CreatureScript
{
    private int _phase;

    public override void Initialize(ITransientScriptedEntity entity, in CreatureSpawn args)
    {
        base.Initialize(entity, in args);
        _phase = 1;   // every field, every time — see Usage Notes
    }

    public override void OnAggro() => Debug.Log("How fortunate, warm meat!");

    public override int OnDamageTaken(int damage) => _phase == 3 ? damage * 2 : damage;
}
```

### 4. Boot once, then call

```csharp
using Sagittaras.Scripting.Building;
using Sagittaras.Scripting.Discovery;

IScriptManager scripts = new ScriptRegistryBuilder()
    .Discover(typeof(OnyxiaScript).Assembly)
    .Build();

// ...on spawn
CreatureSpawn args = new(level: 60);
scripts.Instantiate<CreatureScript, CreatureSpawn>(creature, in args);

// ...from the game loop
scripts.Call<CreatureScript>(creature, script => script.OnAggro());
int damage = scripts.Call<CreatureScript, int>(creature, script => script.OnDamageTaken(100));
```

A creature with no script is not an error. `Call` simply does nothing, the value-returning overload falls back
to `default` — or to a value the call site hands in — and the core keeps the behavior it would have had anyway.

## The three kinds of script

What a script is **bound to** decides how many instances exist, how long they live, and how many of them answer
a single call. Everything else in the package follows from that one choice.

| Kind | Bound to | Instances | Answering one call |
|---|---|---|---|
| `IGlobalScript` | nothing | one, for the whole run | every registered script |
| `IEntityScript` | an entity identifier | one, shared by all entities of that kind | at most one |
| `ITransientEntityScript<TArgs>` | one living entity | one per entity instance | at most one |

### Global scripts — `IGlobalScript`

A hook-point with no entity target — loot rolls, difficulty scaling, save migrations. A global hook-point is a
**broadcast**: every registered script is called, ordered by `[ScriptPriority]`.

```csharp
[GameScript]
public abstract class DifficultyScript : IGlobalScript
{
    public virtual int OnIncomingDamage(int damage) => damage;

    // Zero is "no opinion here" - the next script is asked.
    public virtual int OnDamageCapped() => 0;
}

[ScriptPriority(100)]                       // higher runs first; the default is 0
public class NightmareScript : DifficultyScript
{
    public override int OnIncomingDamage(int damage) => damage * 3;
}
```

The value-returning overload stops at the **first script that answers** — the first non-default result — which is
what the priority is there to decide.

### Shared entity scripts — `IEntityScript`

A script shared by every entity carrying the identifier, so it must keep no state of any single one. The target
comes from `[ScriptedEntity]`, matched against `IScriptedEntity.Id`.

```csharp
[GameScript]
public abstract class LootableScript : IEntityScript
{
    public virtual int OnGoldLooted(int gold) => gold;
}

[ScriptedEntity("chest_small")]
[ScriptedEntity("chest_large")]             // one script may serve several entities
public class ChestLootScript : LootableScript
{
    public override int OnGoldLooted(int gold) => gold * 2;
}
```

An entity holds **at most one script per entrypoint** — two scripts answering the same hook-point for the same
entity would be a conflict rather than a broadcast, so the second registration is refused. The limit is per
entrypoint, not per entity: being lootable and being interactive are separate contracts and may both be served.

### Per-instance scripts — `ITransientEntityScript<TArgs>`

A script created per living instance of an entity, free to keep the state of that one creature. `TArgs` is bound
by the entrypoint, so the call site cannot pass arguments the script was not written for. An entrypoint that
offers nothing but the entity itself binds `NoArgs` and spawns through the shorter overload.

```csharp
scripts.Instantiate<CreatureScript, CreatureSpawn>(creature, in args);
scripts.Instantiate<SilentScript>(creature);        // entrypoint bound to NoArgs
```

State is taken in `Initialize` rather than through a constructor. That is what lets the instance be pooled — and
what makes overwriting every field there a requirement rather than a suggestion.

The instance is discarded when the entity raises `ITransientScriptedEntity.Released`. Raise it whenever the entity
ends its life, including when it returns to an object pool: the same object comes back as a different creature,
and the previous script must not come with it.

## Registering the scripts

Registration is the expensive half on purpose, and it happens once while the game boots. Resolving the
entrypoints of a script, ordering by priority and capturing how a transient script is created are all settled
here, so that reaching a hook-point later is a dictionary lookup and a delegate call.

Both ways in end at the same `ScriptRegistryBuilder`, and `Build()` hands back a manager that is read-only for
the rest of the run.

### By discovery — `ScriptDiscovery`

Finds the scripts of an assembly and registers them. `builder.Discover(assembly)` is the shorthand; the two halves
are separate on purpose.

```csharp
ScriptDiscovery discovery = new();
IEnumerable<ScriptDescriptor> found = discovery.Scan(assembly);
discovery.Register(builder, found);
```

Scanning is reflection, which an AOT build with stripped code cannot be trusted with. `ScriptDescriptor` is the
seam: produce the descriptors any other way — a source generator, a hand-written list, `ScriptDiscovery.Describe`
for a single type — and registration stays exactly the same.

Pass an `IScriptFactory` to have a container create the global and shared entity instances; they are built once
while the game boots, so unlike transient scripts they may take their dependencies through the constructor.

### By hand — `ScriptRegistryBuilder`

```csharp
IScriptManager scripts = new ScriptRegistryBuilder()
    .AddGlobal(new NightmareScript())
    .AddEntity("chest_small", new ChestLootScript())
    .AddTransient<OnyxiaScript>("boss_onyxia")
    .Build();
```

Global and shared entity scripts are registered as **instances**, because that is what they stay. A transient one
is registered as a **type**, because its instances are created later, on the spawns — which is also why it needs a
parameterless constructor, captured here while the concrete type is still known statically.

Registering a script no entrypoint could ever reach, or a second script for a hook-point an entity already has,
is refused with an `ArgumentException` naming what collided. A refused registration leaves the builder untouched.

## Calling the hook-points

`IScriptManager` is the whole surface the core module ever touches. Which overload applies is decided by the
entrypoint and by how the entity is typed at the call site.

```csharp
// No target: every registered script is called.
scripts.Call<DifficultyScript>(script => script.OnIncomingDamage(10));
int capped = scripts.Call<DifficultyScript, int>(script => script.OnDamageCapped());

// A kind of entity: the script shared by all chests of that id.
int gold = scripts.Call<LootableScript, int>(chest, script => script.OnGoldLooted(10));

// One living creature: the script created for this very instance.
scripts.Call<CreatureScript>(creature, script => script.OnAggro());
```

A transient entrypoint cannot be passed to the shared-entity overload — `ITransientEntityScript` does not derive
from `IEntityScript`, so the mistake is a compile error rather than a call that quietly reaches the wrong registry
and finds nothing.

Every overload is safe to call when nothing is registered: the `Action` form does nothing and the `Func` form
returns `default`. Where that is the wrong answer, hand in one of your own:

```csharp
int cap = scripts.Call<DifficultyScript, int>(script => script.OnDamageCapped(), 100);
```

The value only decides what comes back when **nobody answered** — it says nothing about what counts as an
answer. A script never sees it, so passing the question on stays what it always was: returning the default of
the result type. For the entity overloads there is nobody else to ask anyway, so whatever the single script
answers is final and only its absence yields the value.

That leaves `null` free to mean "no opinion" for a hook-point answering with a reference type, while the call
site still gets something usable back:

```csharp
// Every script returning null leaves the title to the core.
string? title = scripts.Call<DifficultyScript, string?>(script => script.OnTitleChosen(), "Normal");
```

### Reaching the script itself

Where a hook-point is not the right shape — holding the script across several calls, or skipping work that only
pays off once somebody is there to see it — the script can be asked for directly.

```csharp
if (scripts.HasScript<CreatureScript>(creature))
{
    // Gathering the context is only worth it when a script will read it.
}

CreatureScript? script = scripts.GetScript<CreatureScript>(creature);
```

`GetScript` is singular for the entity overloads, because an entity holds at most one script per entrypoint. A
global hook-point is a broadcast, so it answers with `GetScripts` instead — every script, ordered the way a
call reaches them.

Prefer `Call` for a single hook-point: it does the same lookup and needs no null check of its own.

## Creating and recycling the instances

`IScriptActivator` provides and takes back the transient instances — the only scripts that come and go while
the game runs.
**A manager pools them by default**, so spawns do not allocate once the game is warm.

```csharp
PoolingScriptActivator pool = new();
pool.Prewarm<OnyxiaScript>(8);              // fill before the first wave spawns

IScriptManager scripts = new ScriptRegistryBuilder()
    .Discover(assembly)
    .WithActivator(pool)
    .Build();
```

`new PoolingScriptActivator(capacity)` caps each pool; a capacity of `0` keeps nothing, which turns pooling off
entirely. `Clear()` drops what the pools hold, worth doing when a scene unloads.

Replacing the activator is also the seam for a container that injects dependencies into a created script. It
cannot inject through the constructor — that is the price of the instance being poolable — but property and
method injection reach it fine.

## Usage Notes

- **`Initialize` must overwrite every field of the script.** Transient scripts are pooled by default, so an
  instance arrives carrying the state of the creature it served before. Whatever the initialization does not
  overwrite is inherited by the next one — a stale phase, a damage counter that never resets. While hunting such a
  bug, `new PoolingScriptActivator(0)` takes pooling out of the picture and tells a leftover state apart from a
  defect of the script itself.
- **Forgetting `[GameScript]` on a base script silently unregisters everything below it.** A type deriving from no
  entrypoint is skipped by discovery rather than reported, because nothing could ever call it. The flip side is
  that a missing attribute looks exactly like scripts that were never written — if a whole module seems ignored,
  start there.
- **`InstanceId` only has to be unique within an entity identifier.** Two different creatures may both be the
  first of their kind; the pair is what identifies the instance. In Unity, `GetInstanceID()` satisfies this
  comfortably.
- **`[ScriptPriority]` only means something for global scripts.** Entity and transient hook-points hold at most one
  script per entrypoint, so there is nothing to order there and the attribute is ignored.
- **Registration is the expensive half, on purpose.** Resolving the entrypoints of a script, ordering by priority
  and capturing how a transient script is created all happen while the game boots, so a hook-point reached from the
  game loop is a dictionary lookup and a delegate call. Build once at startup and share the manager.
- **Nothing here is safe for concurrent use.** Registration, spawns and calls all belong to the thread that owns
  the game loop.

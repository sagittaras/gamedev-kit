# Commands

A command is a type. It declares its name, its description and the values it takes in its constructor, and does
its work in a single method. There are three shapes to write one in, depending on what the command does:

| Base | Implements | For |
|---|---|---|
| `Command` | `ISyncCommand` | Work done within the call — toggling a flag, printing a state, spawning an entity |
| `AsyncCommand` | `IAsyncCommand` | Work that has to be awaited — loading a scene, asking the game server |
| `CommandGroup` | `ISyncCommand` | A branch of the hierarchy, holding the commands that do the work |

## A command doing the work — `Command`

```csharp
public class GodCommand : Command
{
    private static readonly Flag Off = new("off", "Turn the mode off instead of on");

    public GodCommand() : base("god", "Makes the player unkillable")
    {
        Accepts(Off);
    }

    public override ICommandOutput Invoke(ICommandInput input)
    {
        bool enabled = !input.GetValue<bool>(Off);
        Player.Local.IsInvulnerable = enabled;

        return CommandOutput.Success(enabled ? "God mode on." : "God mode off.");
    }
}
```

The parameters are declared once, as fields, and read back through the very same instances — `ICommandInput`
knows no names, so a typo in one cannot survive the compilation. `Accepts` takes arguments, options and flags in a
single call; arguments keep the order they are declared in, as that is the order the line is expected to carry
them in. See [Parameters](./parameters.md) for everything they can declare.

By the time `Invoke` is reached, everything the line could be wrong about has been decided: the command was found,
the values converted and checked, the defaults filled in. What is left to fail belongs to the command — a missing
entity, a scene that is not loaded — and is answered with an output rather than an exception:

```csharp
if (Scene.Active == null)
{
    return CommandOutput.Failure("There is no scene to spawn into.");
}
```

### Reading the values

```csharp
public interface ICommandInput
{
    bool IsSpecified(IParameter parameter);
    T GetValue<T>(IParameter parameter);
    bool TryGetValue<T>(IParameter parameter, out T value);
}
```

- **`GetValue`** answers with the value the line carried, or the default of the parameter when it left it out. It
  throws only for a parameter the command does not declare — which is a mistake in the command, not in the line.
- **`TryGetValue`** answers with false instead of throwing, for a body shared by commands that do not all declare
  the same parameters.
- **`IsSpecified`** tells a value that was typed from one that was defaulted to, which the value alone cannot —
  a flag left out and a flag named as `--off=false` both read as false.

## Work that has to be awaited — `AsyncCommand`

```csharp
public class LoadSceneCommand : AsyncCommand
{
    private static readonly Argument<string> Scene = new("scene", "Scene to load");

    public LoadSceneCommand() : base("load", "Loads a scene")
    {
        Accepts(Scene);
    }

    public override async ValueTask<ICommandOutput> InvokeAsync(ICommandInput input, CancellationToken cancellationToken = default)
    {
        string scene = input.GetValue<string>(Scene);
        await SceneLoader.LoadAsync(scene, cancellationToken);

        return CommandOutput.Success($"Loaded {scene}.");
    }
}
```

It is written exactly like a `Command`, and reached only through `ExecuteAsync` of the manager. The synchronous
`Execute` refuses it with an `InvalidOperationException` rather than blocking on it — blocking on the thread a
console tends to live on would deadlock instead of wait. See [Registry](./registry.md#running-a-line).

Reach for it only when the work genuinely leaves the calling frame; anything answering right away is better off
synchronous.

## A branch of the hierarchy — `CommandGroup`

```csharp
public class SpawnCommand : CommandGroup
{
    public SpawnCommand() : base("spawn", "Spawns things into the active scene")
    {
        Add(new SpawnEntityCommand());
        Add(new SpawnItemCommand());
    }
}
```

A group holds the commands reached by naming them behind it — `spawn entity`, `spawn item`. It is a type of its
own, so a branch has somewhere to keep its description and its options, and the hierarchy is read from the
constructors of the commands rather than assembled a second time elsewhere.

Named alone — `spawn` — a group answers with a [`UsageOutput`](./outputs.md#usageoutput) listing what it holds.
That is not a mistake of the line as much as an unfinished one. A group with something better to say — a state of
its own, a default subcommand — overrides `Invoke`.

### Options of a group

A group may declare options, though never positional arguments. Such an option belongs to the group alone: it is
read when the line stops at the group, by an `Invoke` the group overrides.

```csharp
public class SpawnCommand : CommandGroup
{
    private static readonly Flag List = new("list", "List what can be spawned instead", "l");

    public SpawnCommand() : base("spawn", "Spawns things into the active scene")
    {
        Accepts(List);
        Add(new SpawnEntityCommand());
        Add(new SpawnItemCommand());
    }

    public override ICommandOutput Invoke(ICommandInput input)
    {
        return input.GetValue<bool>(List)
            ? CommandOutput.Success(string.Join(", ", Spawnables.Names))
            : base.Invoke(input);
    }
}
```

`spawn --list` lists what can be spawned, and falling back to `base.Invoke` keeps the usage for `spawn` named alone.

The option is **not inherited** by the subcommands. The parser binds the options to the command the line arrived
at, and the path comes first — so `spawn entity goblin --list` is refused with *"Command 'spawn entity' knows no
option named '--list'."*, and `spawn --list entity goblin` because the subcommand was named behind an option.

### An option shared by the subcommands

An option several subcommands take is declared once, as a field — the group is a natural home for it — and
accepted by every subcommand that takes it. Each of them reads it back through the very same instance:

```csharp
public class SpawnCommand : CommandGroup
{
    public static readonly Flag DryRun = new("dry-run", "Report the work without doing it");

    // ...
}

public class SpawnEntityCommand : Command
{
    private static readonly Argument<string> Entity = new("entity", "Entity to spawn");

    public SpawnEntityCommand() : base("entity", "Spawns an entity into the active scene")
    {
        Accepts(Entity, SpawnCommand.DryRun);
    }

    public override ICommandOutput Invoke(ICommandInput input)
    {
        string entity = input.GetValue<string>(Entity);

        return input.GetValue<bool>(SpawnCommand.DryRun)
            ? CommandOutput.Success($"Would spawn {entity}.")
            : CommandOutput.Success($"Spawned {entity}.");
    }
}

public class SpawnItemCommand : Command
{
    private static readonly Argument<string> Item = new("item", "Item to spawn");

    public SpawnItemCommand() : base("item", "Spawns an item on the ground")
    {
        Accepts(Item, SpawnCommand.DryRun);
    }

    // ... reads SpawnCommand.DryRun the same way
}
```

`spawn entity goblin --dry-run` and `spawn item sword --dry-run` both take it, and the
[help](./help.md) lists it under each of them. A parameter is description rather than storage, so one instance can
serve any number of commands. The group itself takes the option only when it accepts it as well — declaring the
field on the group is where it lives, not what makes the group read it.

## One or the other, never both

A command either holds subcommands or reads positional arguments. With both, a word behind it — `spawn entity` —
would be the name of a subcommand and the first value at once, and no rule telling them apart is worth the surprise
it causes.

The rule is carried by the base a command is written as:

- `Command` and `AsyncCommand` have no `Add` at all, so a subcommand cannot be given to one.
- `CommandGroup` refuses a positional argument with an `InvalidOperationException`, whichever of the two is
  declared first.

Options are free of the rule — they carry a calling sign of their own and can never be mistaken for the name of a
subcommand, which is why a group may declare them; see [Options of a group](#options-of-a-group).

## Names

A name is a single word — the line is cut into words before anything is matched, so a name holding white space
could never be met as a whole, and is refused where it is declared. Names are matched ignoring the case, and have
to be unique among their siblings the same way; `Add` refuses a second `Entity` next to an `entity`.

Since a name only has to be unique among its siblings, the errors of the parser name a command by the whole path
the line walked — *"Command 'spawn entity' needs the argument 'entity'"* — rather than by its own name alone.

## Running a command without a parser — `CommandInput`

A command sees nothing of the text it came from, so it can be run without a line at all. `CommandInput` composes
the values by hand — for a button in the editor tooling, a startup script, or a test:

```csharp
ICommandInput input = new CommandInput(spawnEntity, new Dictionary<IParameter, object?>
{
    [SpawnEntityCommand.Entity] = "goblin",
    [SpawnEntityCommand.Count] = 3,
});

ICommandOutput output = spawnEntity.Invoke(input);
```

Parameters left out fall back to their defaults and are reported as not specified. The values given are held to
the same rules a line is, and an `ArgumentException` names the one that broke:

- a required argument has to be given;
- a value has to be meant for a parameter the command declares;
- a value has to be of the declared type — and `null` only where the type can hold it;
- a value has to pass the [validator](./parameters.md#judging-a-value--validator) of its parameter.

A parameter's own [parser](./parameters.md#reading-a-value-your-own-way--parser) is not involved — the values are
already values, there is no text to read.

## Writing a command by hand

The bases are a convenience, not a requirement — `ICommand` together with `ISyncCommand` or `IAsyncCommand` can
be implemented directly. Nothing then keeps such a command from breaking the rules above, so the
[builder](./registry.md#what-registration-checks) checks every command it is given, and the whole hierarchy below
it.

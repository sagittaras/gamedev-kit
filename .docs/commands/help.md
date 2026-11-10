# Help

The package comes with a help that describes whatever the manager was built with — the commands there are, and
everything about any one of them. It answers with an output of its own, so a console with an interface can lay the
help out its own way, while one without it prints it as text.

## Asking for it

```csharp
ICommandManager commands = new CommandRegistryBuilder()
    .Add(new SpawnCommand())
    .Add(new GodCommand())
    .WithHelp()
    .Build();
```

| Line | Answers with |
|---|---|
| `help` | every command there is, the help itself among them |
| `help spawn` | a branch — its usage, its subcommands and its options |
| `help spawn entity` | a command — its usage from the top of the hierarchy, its arguments and its options with their defaults |

The help is created when the manager is built, so it describes every command registered by then, including the
ones added after `WithHelp` was called. It answers to `HelpCommand.CommandName`, `help`, and building fails with an
`InvalidOperationException` when a registered command already bears that name.

## What it answers with — `HelpOutput`

```csharp
public class HelpOutput : CommandOutput
{
    public IReadOnlyList<ICommand> Path { get; }
    public ICommand? Command { get; }
    public IReadOnlyList<ICommand> Commands { get; }

    public static HelpOutput Overview(IReadOnlyList<ICommand> commands);
    public static HelpOutput About(IReadOnlyList<ICommand> path);
}
```

- **`Path`** leads from the top of the hierarchy down to the command described — `spawn`, `entity` — and is empty for
  the overview. A command does not know the branch it hangs under, and a usage reading `entity <entity>` could not
  be typed; the path is what lets it read `spawn entity <entity>`.
- **`Command`** is the command described, or `null` for the overview. It is always the registered command, with its
  real parameters.
- **`Commands`** are the commands the help lists — all of them for the overview, the subcommands of a branch, none
  for a command doing the work.

Its exit code is `0` — unlike a branch named alone, a help is a successful answer.

### Its plain reading

`help`:

```
Available commands:
  spawn  Spawns things into the active scene
  god    Makes the player unkillable
  help   Describes the commands there are, or the one named behind it
```

`help spawn`:

```
spawn - Spawns things into the active scene

Usage: spawn <subcommand> [options]

Subcommands:
  entity  Spawns an entity into the active scene
  item    Spawns an item on the ground

Options:
  --list, -l  List what can be spawned instead
```

`help spawn entity`:

```
spawn entity - Spawns an entity into the active scene

Usage: spawn entity <entity> [count] [options]

Arguments:
  entity  Entity to spawn
  count   How many to spawn (default: 1)

Options:
  --scene, -s <value>  Scene to spawn into (default: active)
  --mode <value>       Behaviour of the spawn (default: Idle)
  --dry-run            Report the work without doing it
```

A required argument reads `<name>` and an optional one `[name]`. A single-letter option is spelled with one dash and
the rest with two. The default of a flag is not repeated, as a flag is off until named, and every default is spelled
with the invariant culture — the way the line is read, so it could be typed back.

## Showing it your own way

Everything the plain reading is made of is on the commands themselves:

```csharp
case HelpOutput help when help.Command is ICommand command:
    string path = string.Join(" ", help.Path.Select(step => step.Name));
    view.Title(path, command.Description);

    foreach (IArgument argument in command.Arguments)
    {
        view.Row(argument.Name, argument.Description, argument.IsOptional ? argument.DefaultValue : null);
    }

    foreach (IOption option in command.Options)
    {
        view.Row(option.Name, option.Description, option.IsFlag ? null : option.DefaultValue);
    }

    foreach (ICommand subcommand in help.Commands)
    {
        view.Link($"help {path} {subcommand.Name}", subcommand.Name, subcommand.Description);
    }

    break;
```

## How the path is read

The path behind the help can be as long as the hierarchy is deep, while positional arguments come in a fixed
number. So rather than taking the path as values, the help holds a mirror of the whole hierarchy as its
subcommands, and `help spawn entity` is walked by the parser like any other line. It gets everything any other line
gets:

- names matched ignoring the case — `HELP Spawn ENTITY` describes the same command;
- the path stamped as commands, for a console to colour while it is typed;
- a mistyped name refused and pointed at — `help spawn vehicle` answers *"Command 'help spawn' holds no subcommand
  named 'vehicle'."*;
- words behind the path refused — `help spawn entity goblin` answers *"Command 'help spawn entity' takes 0
  argument(s)..."*, naming the path of the help rather than the command, which takes two.

The mirrors never answer about themselves, so the output always carries the registered command. The help about
the help — `help help` — describes the help and mirrors nothing further.

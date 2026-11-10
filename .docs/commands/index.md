# Sagittaras.Commands

`Sagittaras.Commands` is the backend of an in-game console — the developer and debugging kind, the one a designer
opens to spawn a creature, skip a quest or load a scene. It takes a line of text, finds the command it names in a
hierarchy of commands, reads the values the command declares, and runs it.

The idea behind it is to keep **reading a line** apart from **running a command**. A command never sees the text it
was typed as — it declares what it takes, and receives values already read, converted and checked. That is what
lets the same command be run from a console line, from a button in the editor tooling or from a startup script,
and what keeps the parser a service the game can replace without touching a single command.

## Installation

| | |
|---|---|
| Namespace | `Sagittaras.Commands` |
| Unity package ([OpenUPM](https://openupm.com/packages/com.sagittaras.gamedevkit.commands/)) | `com.sagittaras.gamedevkit.commands` |
| NuGet package ([nuget.org](https://www.nuget.org/packages/Sagittaras.Commands/)) | `Sagittaras.Commands` |

See [Installation](../index.md#installation) for adding the package to your project.

## Dependencies

| Package | Required |
|---|---|
| [Sagittaras.GuardClauses](../guard-clauses/index.md) | ✅ |

When installing the package manually, make sure the packages listed above are present too.

## Architecture

```
ICommand                          ← what the parser reads: a name, a description, subcommands, parameters
    ├── ISyncCommand                  ← answers within the call          → Command, CommandGroup
    └── IAsyncCommand                 ← answers through a ValueTask      → AsyncCommand

IParameter                        ← a value a command reads
    ├── IArgument                     ← by its place in the line         → Argument<T>
    └── IOption                       ← by its name                      → Option<T>, Flag

ICommandRegistryBuilder           ← collects the commands while the game boots
    └── ICommandManager               ← reads and runs the lines of the console
            ├── ICommandParser            ← cuts a line up, walks the hierarchy, binds the values
            │       └── IValueConverter       ← turns a piece of text into the declared type
            └── ICommandOutput            ← what a command answers with - text, or more than that
```

Commands form a **hierarchy**: `spawn` is a branch holding `entity` and `item`, and `spawn entity goblin` walks it
down to the command that does the work. A command either holds subcommands or reads positional arguments, never
both — otherwise a word behind it would be both the name of a subcommand and a value.

The namespaces follow what a type is about. The root holds what a command *is*; `Parameters` what a value is
declared by; `Invocation` what a single run is handed and answers with; `Parsing` the reading of a line, with
`Tokens`, `Errors` and `Conversion` beside it for what the parsing hands out and takes in; `Building` the
registration; and `Help` the built-in help.

## Getting Started

```csharp
using Sagittaras.Commands;
using Sagittaras.Commands.Building;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parameters;

// A branch of the hierarchy - it does no work itself, it only holds commands that do.
public class SpawnCommand : CommandGroup
{
    public SpawnCommand() : base("spawn", "Spawns things into the active scene")
    {
        Add(new SpawnEntityCommand());
    }
}

// A command doing the work. Its parameters are fields, read back through the very same instances.
public class SpawnEntityCommand : Command
{
    private static readonly Argument<string> Entity = new("entity", "Entity to spawn");

    private static readonly Argument<int> Count = new("count", "How many to spawn", 1)
    {
        Validator = count => count is >= 1 and <= 10 ? null : $"Between 1 and 10 can be spawned at once, not {count}.",
    };

    private static readonly Flag DryRun = new("dry-run", "Report the work without doing it");

    public SpawnEntityCommand() : base("entity", "Spawns an entity into the active scene")
    {
        Accepts(Entity, Count, DryRun);
    }

    public override ICommandOutput Invoke(ICommandInput input)
    {
        string entity = input.GetValue<string>(Entity);
        int count = input.GetValue<int>(Count);

        if (input.GetValue<bool>(DryRun))
        {
            return CommandOutput.Success($"Would spawn {count}x {entity}.");
        }

        // ... the actual spawning
        return CommandOutput.Success($"Spawned {count}x {entity}.");
    }
}
```

```csharp
// Boot once...
ICommandManager commands = new CommandRegistryBuilder()
    .Add(new SpawnCommand())
    .WithHelp()
    .Build();

// ...then run whatever is typed into the console.
ICommandOutput output = commands.Execute("spawn entity goblin 3 --dry-run");

console.Print(output.Content); // Would spawn 3x goblin.
```

A line the parser cannot read — `spawn entity goblin 50`, `spwan entity` — is answered with an output carrying the
reason rather than with an exception, so `Execute` always hands the console something to print.

## Pages

| Page | Description |
|---|---|
| [Commands](./commands.md) | Writing a command, a branch of the hierarchy, and running a command without a parser |
| [Parameters](./parameters.md) | Arguments, options and flags, with parsers and validators of their own |
| [Parsing](./parsing.md) | The syntax of a line, what reading it gives back, and the types a value is read as |
| [Registry](./registry.md) | Building the manager, and reading and running the lines of the console |
| [Outputs](./outputs.md) | What a command answers with, and how a console tells the answers apart |
| [Help](./help.md) | The built-in help and the output it answers with |

## Usage Notes

- **Registration is the expensive half, on purpose.** Everything that can be said about a command before anybody
  types anything — that it can be run, that no two siblings share a name, that nothing holds subcommands next to
  arguments — is checked while the manager is built. A line typed into the console is then only matched. Build
  once at startup and share the manager.
- **Reading a line is safe from several threads.** Neither the manager nor the parser keeps any state between two
  lines, which matters for a game server taking commands from several administrators at once. Whether *running*
  them is safe is up to the commands themselves.
- **`Parser` and `Validator` are init properties.** Setting them where a parameter is declared needs C# 9, which
  Unity supports from 2021.2 on.
- **Optional arguments come last.** Values go to the arguments in the order they are declared, so an optional
  argument in front of a required one would always take the value meant for it. The builder refuses a command
  declaring them that way.
- **Defaults are never parsed nor judged.** They come from the declaration rather than from the input, so neither
  a parameter's own parser nor its validator ever sees them.

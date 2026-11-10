# Registry

Commands are registered once, while the game boots, and read afterwards for as long as it runs. The builder
collects them and checks everything that can be checked before anybody types anything; the manager it builds is
then only read — a line typed into the console costs a walk of the hierarchy and nothing else.

```
ICommandRegistryBuilder   ← Add, WithParser, WithHelp - while the game boots
    └── ICommandManager       ← Parse, Execute, ExecuteAsync - for every line typed
```

## Building the manager — `CommandRegistryBuilder`

```csharp
ICommandManager commands = new CommandRegistryBuilder()
    .Add(new SpawnCommand())
    .Add(new GodCommand())
    .Add(new LoadSceneCommand())
    .WithParser(new CommandParser(new GameValueConverter()))
    .WithHelp()
    .Build();
```

- **`Add`** registers a command reachable by naming it first — the root of a branch, or a command standing on its
  own. Its subcommands and parameters come with it, declared in its constructor; there is nothing to assemble here.
- **`WithParser`** replaces the way the lines are read — a parser of another syntax, or the default one carrying a
  [converter of the game's own](./parsing.md#a-converter-of-the-games-own).
- **`WithHelp`** adds the [built-in help](./help.md). It is created by `Build`, when every command it is to describe
  is known, so it also describes the commands registered after the call.
- **`Build`** copies the registrations, so a command added to the builder later never reaches a manager already
  built. Every manager gets a help of its own, over its own commands.

What used to be the root of a hierarchy in a command-line library is the manager itself: it holds the commands the
first word is matched against, and the parsing is a service it asks.

## What registration checks

`Add` checks the command it is given and the whole hierarchy below it, and refuses with an
`InvalidOperationException`:

- a command whose name is already registered, ignoring the case — only one of the two could ever be reached;
- a command implementing neither `ISyncCommand` nor `IAsyncCommand` — the parser could find it, and nobody could
  run it;
- a command holding subcommands next to positional arguments;
- a command declaring a required argument behind an optional one — values go to the arguments in order, so the
  optional one would always take the value meant for the required one;
- two subcommands of the same name, ignoring the case.

The bases of the package cannot be written in any of those ways, so the checks are there for a command
[written by hand](./commands.md#writing-a-command-by-hand) — and they are made while the game boots rather than
found out by a console line that quietly does nothing.

`Build` refuses the help when a registered command already answers to its name, `help`.

## Reading a line without running it

```csharp
IParseResult result = commands.Parse("spawn entity goblin --scene");
```

`Parse` reads the line and stops there. It is the call for everything a console does before a line is committed
to — [colouring it by its tokens](./parsing.md#what-reading-gives-back--iparseresult) while it is typed, offering the
subcommands of the branch it arrived at, telling what it would do — and for telling a synchronous command from an
asynchronous one before choosing how to run it. The result can then be run without being read a second time.

## Running a line

```csharp
ICommandOutput output = commands.Execute("god --off");
ICommandOutput awaited = await commands.ExecuteAsync("load arena", cancellationToken);
```

|  | `Execute` | `ExecuteAsync` |
|---|---|---|
| a synchronous command | runs it | runs it, the task already completed |
| an asynchronous command | throws `InvalidOperationException` | awaits it |
| a line that cannot be read | answers with a [`RefusalOutput`](./outputs.md#refusaloutput) | answers with a `RefusalOutput` |

`Execute` refuses an asynchronous command instead of blocking on it — waiting for a task on the thread a console
tends to live on deadlocks rather than waits. A console with an awaiting loop needs nothing but `ExecuteAsync`;
one that does not can tell the two apart from the parsed line:

```csharp
IParseResult result = commands.Parse(line);

ICommandOutput output = result.Command is IAsyncCommand
    ? await commands.ExecuteAsync(result, cancellationToken)
    : commands.Execute(result);
```

Both entrypoints take either the line or a result already parsed. Neither throws for a line it cannot read: a
typo is answered with an output a console can print, not with a stack trace.

## Exit codes

| Code | Answered by |
|---|---|
| `0` | a command that did its work — `CommandOutput.Success`, the help |
| `1` | a branch named alone — `UsageOutput.BranchExitCode` — and `CommandOutput.Failure` unless given another |
| `2` | a line that could not be read — `RefusalOutput.UsageExitCode` |
| any other | a command failing with a code of its own — `CommandOutput.Failure(reason, code)` |

A branch named alone and a command failing without a code of its own share `1`; a console that has to tell them
apart does so by the [type of the output](./outputs.md).

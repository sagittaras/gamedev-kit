# Outputs

What a command answers with does not have to be a line of text. A lookup in the game database answers best with a
table, a help with sections, a refused line with the word that was wrong — and how any of that is shown is the
game's business, not the package's. The package only makes sure every answer can carry more than text, and can
always be read as text by whoever does not know what more it carries.

## The contract — `ICommandOutput`

```csharp
public interface ICommandOutput
{
    int ExitCode { get; }
    string Content { get; }
}
```

- **`ExitCode`** is zero for success and anything else for a failure — see [Registry](./registry.md#exit-codes)
  for the codes the package answers with itself.
- **`Content`** is the plain reading of the answer, and it is always there. A console with an interface of its own
  shows an output its own way; a log, the socket of a dedicated server, a test, or a console that has never heard of
  the output's type prints `Content` — so an output is never lost, only shown plainly.

## Plain answers — `CommandOutput`

```csharp
return CommandOutput.Success("Spawned 3x goblin.");
return CommandOutput.Success();                                  // the work speaks for itself
return CommandOutput.Failure("There is no scene to spawn into.");  // exit code 1
return CommandOutput.Failure("The server did not answer.", 4);     // a code of its own
```

A failure is what the command itself ran into — a missing entity, a scene that is not loaded. What the line was
wrong about never reaches a command. `Failure` refuses a zero exit code, as zero is what a success is told by.

## An output of the game's own

`CommandOutput` is a class with a protected constructor, to be derived from: carry the payload, and keep the
content a plain reading of the same answer.

```csharp
public class ItemTableOutput : CommandOutput
{
    public ItemTableOutput(string query, IReadOnlyList<Item> items)
        : base(0, Describe(query, items))
    {
        Query = query;
        Items = items;
    }

    public string Query { get; }

    public IReadOnlyList<Item> Items { get; }

    private static string Describe(string query, IReadOnlyList<Item> items)
    {
        return items.Count == 0
            ? $"No item matches '{query}'."
            : string.Join(Environment.NewLine, items.Select(item => $"{item.Id,6}  {item.Name}"));
    }
}
```

```csharp
public override ICommandOutput Invoke(ICommandInput input)
{
    string query = input.GetValue<string>(Query);

    return new ItemTableOutput(query, Database.Items.Where(item => item.Name.Contains(query)).ToList());
}
```

A console that knows the type lays the items out as a table and highlights the query in every name; one that does
not prints the lines of `Content`.

An output meant to be shown generically — a table of anything, rather than of items — is best given a non-generic
face next to its generic class: an `ITableOutput` with the columns and the cells as text, implemented by a
`TableOutput<T>` the commands create. A console cannot match `TableOutput<>` for every `T`, but it can match the
interface, while a console wanting more still reaches for the typed class. It is the same split the package makes
between `Parameter<T>` and `IParameter`.

## Outputs of the package

The package answers with outputs of its own in three places, and they double as the pattern above.

### `UsageOutput`

A [branch](./commands.md#a-branch-of-the-hierarchy--commandgroup) named alone answers with what it holds. The
output carries the branch itself in `Command`, so a console can lay its subcommands out as a help of its own
making; `Content` lists them with their descriptions lined up:

```
Command 'spawn' has to be followed by one of:
  entity  Spawns an entity into the active scene
  item    Spawns an item on the ground
```

Its exit code is `UsageOutput.BranchExitCode` — not a success, as the line stopped short of anything to run, but
not a mistake of the line either.

### `RefusalOutput`

A line the parser could not read is answered with the whole failed parsing in `Result`, and its reason in `Error`.
That is what lets a console underline the word the error names, colour the rest of the line, or answer with the
usage of the command the line got as far as naming — straight from `Execute`, without parsing the line a second
time. `Content` is the message of the error; the exit code is `RefusalOutput.UsageExitCode`.

### `HelpOutput`

The answer of the [built-in help](./help.md), carrying the commands it describes.

## Telling them apart

The dispatch is a plain `switch` on the type, the most specific case first and `Content` as the last resort:

```csharp
switch (commands.Execute(line))
{
    case RefusalOutput refusal:
        view.Underline(refusal.Error.Token);
        view.PrintError(refusal.Content);
        break;
    case HelpOutput help:
        view.ShowHelp(help.Path, help.Command, help.Commands);
        break;
    case UsageOutput usage:
        view.ShowHelp(usage.Command);
        break;
    case ItemTableOutput table:
        view.ShowItems(table.Items, highlight: table.Query);
        break;
    case var output:
        view.Print(output.Content);
        break;
}
```

The package ships no registry of formatters on purpose. Matching an output to the way it is shown is what the
switch above does, and the showing itself belongs to the layer that knows the engine — which is where a registry of
that kind would live, if a game ever needs one.

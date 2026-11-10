# Parsing

`ICommandParser` reads a line as one of the commands it is offered, and gives back what it made of it. It never
runs anything — `IParseResult` is the whole of its answer — which is what lets a console show what a line means
before it is committed to.

```csharp
public interface ICommandParser
{
    IParseResult Parse(string input, IReadOnlyList<ICommand> commands);
}
```

The manager asks it for every line; a console reaches it through [`ICommandManager.Parse`](./registry.md#reading-a-line-without-running-it).
The default `CommandParser` reads a line in three steps: it **cuts** the line into words, **walks** the hierarchy
for as long as the words name commands, and **binds** the rest to the parameters of the command it arrived at.

## The syntax of a line

```
spawn entity "ancient guardian" 3 --scene arena --mode hostile --dry-run
└─ path ───┘ └─ arguments ──────┘ └─ options, in any order ────────────┘
```

| Written | Read as |
|---|---|
| `spawn entity goblin` | the path `spawn entity`, then `goblin` as the first argument |
| `"ancient guardian"` | a single value, the quotes left out |
| `--scene arena`, `-s arena` | the option `scene`, by its name or its alias, with the value `arena` |
| `--scene=arena`, `--scene="the arena"` | the same, handed over through an equals sign |
| `--dry-run` | the flag `dry-run`, set |
| `--dry-run=false` | the flag `dry-run`, given an explicit value |
| `-3`, `-.5` | negative numbers, not options |
| `SPAWN Entity` | the same as `spawn entity` — names, aliases and enum members ignore the case |

An option is recognized by a leading dash followed by anything but a digit or a decimal point — one dash or two,
for any name; the [help](./help.md) spells a single letter with one and the rest with two. Short flags are not
bundled: `-abc` is an option named `abc`, not three of them.

### The path comes first

The walk goes down the hierarchy while the words keep naming subcommands, and ends at the first one that does not —
or at the first option. Options and values stand behind the path:

```
spawn entity goblin --dry-run     ✔
spawn --dry-run entity goblin     ✘  Subcommand 'entity' has to be named before the options of 'spawn'.
```

That order is what keeps the parser from having to know whose option it is — and whether it takes a value —
before it knows which command was meant. A branch still reads the options it declares itself, when it is the one
the line arrived at: `spawn --list`.

## What reading gives back — `IParseResult`

```csharp
public interface IParseResult
{
    bool Success { get; }
    ICommand? Command { get; }
    ICommandInput? Input { get; }
    IParseError? Error { get; }
    IReadOnlyList<Token> Tokens { get; }
    IReadOnlyList<Token> UnmatchedTokens { get; }
}
```

- **`Command`** is there whenever the line got as far as naming one — even when it failed afterwards, so a console
  can answer a mistyped value with the usage of the command it was meant for.
- **`Input`** is there only when the whole line was read; it is all the command is ever given.
- **`Tokens`** carries the whole line, each word with the role it turned out to play and its place in the line —
  `Start` and `Length`, the quotes counted — which is what a console colours the line by while it is typed:

```csharp
foreach (Token token in commands.Parse(line).Tokens)
{
    Color colour = token.Type switch
    {
        TokenType.Command => Palette.Command,
        TokenType.Option => Palette.Option,
        TokenType.Argument or TokenType.OptionValue => Palette.Value,
        _ => Palette.Unknown,
    };

    view.Paint(token.Start, token.Length, colour);
}
```

- **`UnmatchedTokens`** are the words that were left without a role — on a failed line, the one the parsing
  stumbled on and whatever came behind it.

Parsing stops at the first thing it cannot make sense of, so there is only ever one error — reporting the second
problem of a line whose first word was already wrong would be guessing.

## Errors — `IParseError`

A mistyped line is the most ordinary thing to happen to a console, so the reason is a value rather than an
exception: a `Code` to answer differently than it is printed, a `Message` meant for whoever typed the line, and the
`Token` it is about — `null` when the line is at fault by what it does not hold.

| Code | Example | The token |
|---|---|---|
| `EmptyInput` | *(nothing)* | — |
| `UnterminatedQuote` | `spawn item "ancient` | the unclosed word |
| `UnknownCommand` | `teleport 3 4` | `teleport` |
| `UnknownSubcommand` | `spawn vehicle` | `vehicle` |
| `MissingArgument` | `spawn entity` | — |
| `TooManyArguments` | `spawn entity goblin 3 extra` | `extra` |
| `UnknownOption` | `spawn entity goblin --colour red` | `--colour` |
| `MissingOptionValue` | `spawn entity goblin --scene` | `--scene` |
| `ValueTypeMismatch` | `spawn entity goblin many` | `many` |
| `ValueRefused` | `summon 50` | `50` |

`ValueTypeMismatch` means the text is not the declared type; `ValueRefused` means it is, and breaks a rule of the
parameter — its own [parser or validator](./parameters.md) refused it, in its own words. An option followed by
another option is `MissingOptionValue` rather than taking the second one for its value — `--scene --dry-run` would
parse, and then quietly do the wrong thing.

The errors name a command by the whole path the line walked — *"Command 'spawn entity' takes 2 argument(s)..."* —
as a name is only unique among its siblings.

## Reading the values

A piece of text becomes the value a parameter declares through `IValueConverter` — unless the parameter carries a
[parser of its own](./parameters.md#reading-a-value-your-own-way--parser).

```csharp
public interface IValueConverter
{
    bool TryConvert(string text, Type valueType, out object? value);
}
```

It answers with false rather than throwing — a value spelled wrong is an ordinary thing to happen, and the parser
turns the answer into a `ValueTypeMismatch` pointing at the word. `DefaultValueConverter` reads:

| Type | Read as |
|---|---|
| `string` | the text as it was typed |
| `bool` | `true`, `yes`, `on`, `1` and `false`, `no`, `off`, `0`, ignoring the case |
| `char` | a single character |
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong` | an integer with an optional sign |
| `float`, `double`, `decimal` | a number with a decimal point and an optional exponent |
| `Guid` | an identifier in any of its usual forms |
| `DateTime` | whatever the runtime reads under the invariant culture |
| any enum | the name of a member, ignoring the case |
| any `Nullable<T>` | as the type it wraps |

Every number is read with the **invariant culture** and **without a thousands separator**. A command typed into a
console is not a document: a build running under a comma-decimal locale must not read `1.5` differently, and `1,5`
has to be refused rather than taken for fifteen — which is what `Convert.ChangeType`, allowing a thousands
separator, would make of it.

An enum is read by the **names of its members only**, never by a number — `--mode 7` would otherwise produce a value
the enum does not declare.

A date is read by whatever `DateTime.TryParse` reads under the invariant culture: an ISO date with or without its
time, a month spelled by name, or a time alone meaning today. Numeric dates are read **month first** —
`21.09.2026` is refused, but `01.02.2026` is the second of January. A date meant to be read the same by everyone is
best written the ISO way, `2026-02-01`.

## A converter of the game's own

A type the whole game shares — a colour, an entity identifier, a vector — is taught to the parser through a
converter of its own, falling back to the default one for everything else:

```csharp
public class GameValueConverter : IValueConverter
{
    private readonly DefaultValueConverter _fallback = new();

    public bool TryConvert(string text, Type valueType, out object? value)
    {
        if (valueType == typeof(Color) && ColorUtility.TryParseHtmlString(text, out Color colour))
        {
            value = colour;
            return true;
        }

        return _fallback.TryConvert(text, valueType, out value);
    }
}
```

```csharp
ICommandManager commands = new CommandRegistryBuilder()
    .Add(new TintCommand())
    .WithParser(new CommandParser(new GameValueConverter()))
    .Build();

commands.Execute("tint --colour #ff8800");
```

Where a rule belongs:

| The rule is about | Put it in |
|---|---|
| a type the whole game reads the same way | a converter of the game's own |
| how a single parameter reads its text | the [parser](./parameters.md#reading-a-value-your-own-way--parser) of that parameter |
| which values of the type a single parameter accepts | the [validator](./parameters.md#judging-a-value--validator) of that parameter |

## Replacing the parser

`ICommandParser` can be implemented from scratch for a syntax of another kind, and handed in through
`WithParser`. It is shared by every line the manager reads, so it should keep no state between two of them, and it
answers with `ParseResult.Parsed` or `ParseResult.Failed` — the rest of the package, down to the
[`RefusalOutput`](./outputs.md#refusaloutput) a console receives, works with whatever it produces.

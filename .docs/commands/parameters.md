# Parameters

A parameter is a value a command reads. It comes in two kinds, told apart by how the line reaches it — an argument
by its place, an option by its name — and a flag as a special option whose whole value is whether it was named.
Everything about the value itself is the same for all of them, and lives in their common base `Parameter<T>`.

| Type | Reached by | Default |
|---|---|---|
| `Argument<T>` | its place among the remaining words | only when given one — that is what makes it optional |
| `Option<T>` | its name, anywhere behind the path | always |
| `Flag` | its name alone, taking no value | `false` |

Parameters are declared as fields of the command and read back through the very same instances:

```csharp
private static readonly Argument<string> Entity = new("entity", "Entity to spawn");
private static readonly Option<string> Scene = new("scene", "Scene to spawn into", "active", "s");

public override ICommandOutput Invoke(ICommandInput input)
{
    string entity = input.GetValue<string>(Entity);
    string scene = input.GetValue<string>(Scene);
    // ...
}
```

A parameter is description, not storage — the same instance serves every run of the command, so it never holds a
value. It is the key the value is found under.

## Arguments — `Argument<T>`

```csharp
private static readonly Argument<string> Entity = new("entity", "Entity to spawn");       // required
private static readonly Argument<int> Count = new("count", "How many to spawn", 1);       // optional, 1 when left out
```

An argument has no calling sign — it is recognized by its place, so the order it is declared in through `Accepts`
is the order the line is expected to carry it in: `spawn entity goblin 3`.

It is required unless it is given a default, and giving it one is the only way to make it optional. The two could
not be told apart in the line anyway — an argument that may be missing has to answer with something once it is —
and keeping them one decision means every declared parameter always has a value to read. The optional ones come
last: a value always goes to the next argument in order, so an optional one in front would take the value meant
for a required one behind it, and the [builder](./registry.md#what-registration-checks) refuses a command declaring
them that way.

## Options — `Option<T>`

```csharp
private static readonly Option<string> Scene = new("scene", "Scene to spawn into", "active", "s");
private static readonly Option<SpawnMode> Mode = new("mode", "Behaviour of the spawn", SpawnMode.Idle);
```

An option is named in the line — `--scene arena`, `-s arena`, `--scene=arena` — anywhere behind the path of the
command, in any order. The name and the aliases are declared without the dashes the line spells them with.

An option always has a default, as it is always allowed to be left out. One that had to be present would only be
an argument with a name in front of it.

## Flags — `Flag`

```csharp
private static readonly Flag DryRun = new("dry-run", "Report the work without doing it");
```

Naming a flag is the whole of its value — `--dry-run` sets it, leaving it out leaves it `false`. It is a type of its
own rather than an `Option<bool>` that guesses, because `--verbose true` is a perfectly ordinary option and only the
declaration can tell the two apart. A flag still reads an explicit value handed to it through an equals sign —
`--dry-run=false` — as that pairing is spelled out in the line.

## Reading a value your own way — `Parser`

The [converter](./parsing.md#reading-the-values) reads a type and nothing else. A rule of a single parameter — a
word standing for a number, a name that has to exist in the scene — is handed over as the parameter's own parser,
which replaces the converter for that parameter alone:

```csharp
private static readonly Argument<int> Percent = new("percent", "Share of the health to restore")
{
    Parser = text => text switch
    {
        "half" => ParsedValue<int>.Read(50),
        "full" => ParsedValue<int>.Read(100),
        _ when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && value is >= 0 and <= 100
            => ParsedValue<int>.Read(value),
        _ => ParsedValue<int>.Refused($"The share has to be between 0 and 100, and '{text}' is not."),
    },
};
```

The parser answers with a `ParsedValue<T>` — either `Read(value)`, or `Refused(reason)` with the reason in words
meant for whoever typed the line. They reach the console unchanged, under the code `ValueRefused`, which tells a
value breaking a rule apart from a value of the wrong type. `Refused` requires a reason; a parser answering with the
default of the struct, which is a refusal without one, is still answered plainly — *"Argument 'percent' does not
accept 'lots'."*

A parser receives the raw text, so it reads numbers on its own — and has to read them with the invariant culture,
the way the rest of the line is read, or `1,5` turns into fifteen on a comma-decimal keyboard. For a rule over a
number the converter reads well on its own, a validator is the safer tool.

## Judging a value — `Validator`

```csharp
private static readonly Argument<int> Count = new("count", "How many to summon", 1)
{
    Validator = count => count is >= 1 and <= 10 ? null : $"Between 1 and 10 can be summoned at once, not {count}.",
};
```

A validator sees the value already read — by the converter or by the parser — and only judges it: `null` for a value
it accepts, the reason otherwise, reaching the console under `ValueRefused` the same way a refusal of a parser does.
It never sees the text, so it never reads a number differently than the rest of the line is read.

The two compose. The parser reads, and the validator judges what the parser made of the text:

```csharp
private static readonly Option<string> Creature = new("creature", "What to summon", "imp", "c")
{
    Parser = text => ParsedValue<string>.Read(text == "drake" ? "dragon" : text),
    Validator = creature => creature == "dragon" ? "Dragons cannot be summoned." : null,
};
```

`--creature drake` is refused, because the validator sees `dragon`. A parameter holds a single validator; several
rules are one validator checking them in turn.

## What neither of them sees

- **Defaults.** They come from the declaration rather than from the line, so they are never parsed nor judged.
- **A composed input** is [judged by the validators](./commands.md#running-a-command-without-a-parser--commandinput)
  of its parameters, but never parsed — its values are already values.

## The contract behind them — `IParameter`

```csharp
public interface IParameter
{
    Type ValueType { get; }
    string Name { get; }
    string Description { get; }
    bool HasDefaultValue { get; }
    object? DefaultValue { get; }
    Func<string, ParsedValue<object?>>? Parser { get; }
    Func<object?, string?>? Validator { get; }
}
```

The binding works with parameters through this untyped contract; `Parameter<T>` builds the untyped forms of the
parser and the validator once, when they are set, rather than on every value read. `Parameter<T>` cannot be derived
from outside the package — arguments and options are the only two kinds the parser knows how to read, so a third
could be declared but never reached. `Argument<T>` and `Option<T>` stay open to derive from.

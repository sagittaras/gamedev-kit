# Sagittaras.GuardClauses

## Installation

| | |
|---|---|
| Namespace | `Sagittaras.GuardClauses` |
| Unity package ([OpenUPM](https://openupm.com/packages/com.sagittaras.gamedevkit.guard-clauses/)) | `com.sagittaras.gamedevkit.guard-clauses` |
| NuGet package ([nuget.org](https://www.nuget.org/packages/Sagittaras.GuardClauses/)) | `Sagittaras.GuardClauses` |

See [Installation](../index.md#installation) for adding the package to your project.

## Dependencies

This package has no dependencies on other packages from the kit.

---

`Sagittaras.GuardClauses` is a lightweight implementation of the guard clause pattern for defensive
programming. Guard clauses validate input at the start of a method and throw immediately if the input
is invalid — keeping the method body free from nested conditions and making invalid states visible early.

```csharp
// Without guard clauses
public void SetDamage(int value, int max)
{
    if (value < 0)
        throw new ArgumentOutOfRangeException(...);
    if (value > max)
        throw new ArgumentOutOfRangeException(...);

    _damage = value;
}

// With guard clauses
public void SetDamage(int value, int max)
{
    Guard.Against.LessThan(value, 0);
    Guard.Against.GreaterThan(value, max);

    _damage = value;
}
```

## Getting Started

All guard clauses are accessed through the static `Guard.Against` entry point:

```csharp
Guard.Against.LessThan(value, 0);
```

`Guard.Against` returns an `IGuardClause` instance. The actual validation methods are extension methods
on `IGuardClause`, which means you can add your own clauses without modifying the library (see
[Extending Guard Clauses](#extending-guard-clauses) below).

Every clause is named after the thing it guards *against* and returns nothing — it either lets the value
through or throws. A method therefore stacks the clauses it needs and then works with the value it was
given, unguarded from there on:

```csharp
public SpawnCommand(string name, int weight)
{
    Guard.Against.NullOrWhiteSpace(name);
    Guard.Against.WhiteSpaceInside(name);
    Guard.Against.LessThan(weight, 1);

    _name = name;
    _weight = weight;
}
```

## Available Clauses

### Numbers

#### `LessThan(input, threshold)`

Ensures `input` is not less than `threshold`. Throws `ArgumentOutOfRangeException` if `input < threshold`.
Overloaded for `int`, `float` and `double`.

```csharp
Guard.Against.LessThan(health, 0);     // health must be >= 0
Guard.Against.LessThan(speed, 1);      // speed must be >= 1
```

---

#### `GreaterThan(input, threshold)`

Ensures `input` is not greater than `threshold`. Throws `ArgumentOutOfRangeException` if `input > threshold`.
Overloaded for `int`, `float` and `double`.

```csharp
Guard.Against.GreaterThan(health, maxHealth);  // health must be <= maxHealth
Guard.Against.GreaterThan(level, 60);          // level must be <= 60
```

---

#### `OutOfRange(input, min, max)`

Ensures `input` falls within an inclusive range `[min, max]`. Throws `ArgumentOutOfRangeException`
if `input < min` or `input > max`. Overloaded for `int`, `float` and `double`.

```csharp
Guard.Against.OutOfRange(damage, 0, 9999);    // damage must be between 0 and 9999
Guard.Against.OutOfRange(chanceValue, 0, 10000);
```

---

#### `Zero(input)`

Ensures `input` is not zero. Throws `ArgumentOutOfRangeException` if `input == 0`.

For a value where zero carries a meaning of its own elsewhere — an exit code telling a failure from a
success, an amount that has to move something.

```csharp
Guard.Against.Zero(exitCode, "A failure needs a non-zero exit code.");
```

---

#### `LessThanZero(input)`

Ensures `input` is not negative. Throws `ArgumentOutOfRangeException` if `input < 0`.

```csharp
Guard.Against.LessThanZero(elapsedSeconds);
```

---

#### `DivisionByZero(input)`

Ensures `input` is not zero. Throws `DivideByZeroException` if `input == 0`.

The same check as `Zero`, said about a divisor and answered with the exception that belongs to it.

```csharp
Guard.Against.DivisionByZero(divisor);
int result = value / divisor;
```

> This clause is used internally by `DieRoll` to guard the `/` operator.

---

### Strings

#### `NullOrEmpty(input)`

Ensures `input` is neither null nor empty. Throws `ArgumentException` if it is.

```csharp
Guard.Against.NullOrEmpty(playerName);
```

---

#### `NullOrWhiteSpace(input)`

Ensures `input` holds something other than white space. Throws `ArgumentException` if it is null, empty
or white space only.

```csharp
Guard.Against.NullOrWhiteSpace(playerName);
```

---

#### `WhiteSpaceInside(input)`

Ensures `input` is a single word. Throws `ArgumentException` if it holds a white space character.

For values that are later matched, split or spelled as one — an identifier, a key, a name an input is
recognized by. A null or empty value is not what this clause is about and gets through, so guard it with
`NullOrWhiteSpace` beforehand:

```csharp
Guard.Against.NullOrWhiteSpace(commandName);
Guard.Against.WhiteSpaceInside(commandName);
```

> `Sagittaras.Commands` guards the names of its commands, arguments and options with this pair — a name
> holding white space could never be matched by a parser that cuts the input into tokens first.

---

### State

#### `False(value)`

Ensures `value` is not false. Throws `InvalidOperationException` if it is.

For an invariant of the object rather than the value of an argument — which is why it answers with
`InvalidOperationException` and not with one of the `Argument*` family.

```csharp
Guard.Against.False(_registry.IsBuilt, "The registry has to be built before it is asked anything.");
```

---

#### `True(value)`

Ensures `value` is not true. Throws `InvalidOperationException` if it is.

The mirror of `False`, for the times the condition reads better stated as the thing that must not have
happened:

```csharp
Guard.Against.True(_commands.ContainsKey(name), $"A command named '{name}' is already registered.");
```

---

## Extending Guard Clauses

Because the validation methods are extension methods on `IGuardClause`, adding a new clause requires
only a static extension class — no changes to the library needed. Keep to the same shape as the built-in
ones: name the clause after what it guards *against*, take the guarded value first, accept an optional
`message`, return nothing, and throw the exception that fits the kind of mistake being made.

```csharp
public static class GuardClauseGridExtension
{
    /// <summary>
    ///     Ensures the coordinates fall inside the bounds of the grid.
    /// </summary>
    public static void OffGrid(this IGuardClause _, int x, int y, int width, int height, string? message = null)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            throw new ArgumentOutOfRangeException(nameof(x), $"({x}, {y})", message ?? $"Coordinates must fall inside a {width}x{height} grid.");
        }
    }
}
```

Once defined, the new clause is available on `Guard.Against` immediately:

```csharp
Guard.Against.OffGrid(column, row, Width, Height);
```

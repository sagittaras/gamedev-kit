# Sagittaras.GuardClauses

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

## Available Clauses

### `LessThan(input, threshold)`

Ensures `input` is not less than `threshold`. Throws `ArgumentOutOfRangeException` if `input < threshold`.

```csharp
Guard.Against.LessThan(health, 0);     // health must be >= 0
Guard.Against.LessThan(speed, 1);      // speed must be >= 1
```

---

### `GreaterThan(input, threshold)`

Ensures `input` is not greater than `threshold`. Throws `ArgumentOutOfRangeException` if `input > threshold`.

```csharp
Guard.Against.GreaterThan(health, maxHealth);  // health must be <= maxHealth
Guard.Against.GreaterThan(level, 60);          // level must be <= 60
```

---

### `OutOfRange(input, min, max)`

Ensures `input` falls within an inclusive range `[min, max]`. Throws `ArgumentOutOfRangeException`
if `input < min` or `input > max`.

```csharp
Guard.Against.OutOfRange(damage, 0, 9999);    // damage must be between 0 and 9999
Guard.Against.OutOfRange(chanceValue, 0, 10000);
```

---

### `DivisionByZero(input)`

Ensures `input` is not zero. Throws `DivideByZeroException` if `input == 0`.

```csharp
Guard.Against.DivisionByZero(divisor);
int result = value / divisor;
```

> This clause is used internally by `DieRoll` to guard the `/` operator.

---

## Extending Guard Clauses

Because the validation methods are extension methods on `IGuardClause`, adding a new clause requires
only a static extension class — no changes to the library needed.

```csharp
public static class MyGuardExtensions
{
    /// <summary>
    ///     Ensures the string is not null or empty.
    /// </summary>
    public static void NullOrEmpty(this IGuardClause _, string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Value must not be null or empty.", nameof(input));
    }
}
```

Once defined, the new clause is available on `Guard.Against` immediately:

```csharp
Guard.Against.NullOrEmpty(playerName);
```

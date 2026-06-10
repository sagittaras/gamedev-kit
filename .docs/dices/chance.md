# Chance

`Chance` is a value type representing a probability between 0% and 100%. Internally it stores an integer in
the range `0–10000`, where `10000` equals 100%. This avoids floating-point precision issues while still
allowing float/double input.

When passing a `float` or `double`, the value represents a percentage in the range `0.00–100.00`.
The constructor multiplies it by an internal factor (`100`) to convert it to the integer representation.

## Creating a Chance

```csharp
// From float (0.00–100.00) — most readable
Chance c1 = new Chance(30f);    // 30%
Chance c2 = new Chance(100f);   // 100%
Chance c3 = new Chance(0.5f);   // 0.5%

// From double
Chance c4 = new Chance(75.0);   // 75%

// From raw integer (0–10000)
Chance c5 = new Chance(5000);   // 50%

// Implicit conversion — no constructor needed
Chance c6 = 25f;                // 25%
Chance c7 = 2500;               // 25%
```

Values outside `0–10000` (after conversion) throw `ArgumentOutOfRangeException`.

## Static Constants

| Constant | Value | Meaning |
|---|---|---|
| `Chance.Min` | `0` | 0% — never succeeds |
| `Chance.Max` | `10000` | 100% — always succeeds |
| `Chance.Half` | `5000` | 50% |

## Evaluating a Chance

Use `IDiceBag.Try()` to roll against a chance, or the extension method directly on the `Chance` value:

```csharp
IDiceBag dice = DiceBag.Instance;

// Via DiceBag
bool hit = dice.Try(new Chance(40f));

// Via extension method (uses DiceBag.Instance by default)
bool hit = new Chance(40f).Try();

// With a custom DiceBag instance
bool hit = new Chance(40f).Try(myDiceBag);
```

## Weighted Pick

Choose between two values based on a chance:

```csharp
// 70% chance to pick "Hit", 30% to pick "Miss"
string result = new Chance(70f).WeightedPick("Hit", "Miss");

// Equivalent via DiceBag extension
string result = dice.WeightedPick(new Chance(70f), "Hit", "Miss");
```

## Operators

`Chance` supports arithmetic and comparison operators:

```csharp
Chance a = new Chance(0.30f);
Chance b = new Chance(0.20f);

Chance sum  = a + b;   // 50%
Chance diff = a - b;   // 10%
Chance inc  = a + 500; // 35% (raw integer offset)

bool greater = a > b;  // true
bool equal   = a == b; // false
```

## Conversions

`Chance` implicitly converts back to numeric types:

```csharp
Chance c = new Chance(0.50f);

int    raw    = c;  // 5000
float  asFloat = c; // 0.5f
double asDouble = c; // 0.5
```

# RandomRange

`RandomRange` is a value type representing an inclusive integer range `[Min, Max]`. It is used with
`IDiceBag.Next()` to generate a random integer within defined bounds.

## Creating a RandomRange

```csharp
// new RandomRange(min, max)
RandomRange range = new RandomRange(1, 100);   // 1–100
RandomRange fixed = new RandomRange(42);        // always 42 (min == max)

// Tuple implicit conversion
RandomRange range = (1, 100);

// From System.Range (note: end is exclusive in C# ranges, converted to inclusive)
RandomRange range = 1..100;
```

`min` must not be greater than `max`. Passing an invalid range throws `ArgumentOutOfRangeException`.

## Static Constants

| Constant | Range | Use case |
|---|---|---|
| `RandomRange.One` | `[1, 1]` | Fixed value of 1 |
| `RandomRange.Angle` | `[0, 360]` | Random angle in degrees |

## Generating a Value

```csharp
IDiceBag dice = DiceBag.Instance;

RandomRange range = new RandomRange(1, 100);

// Via DiceBag
int value = dice.Next(range);

// Via extension method (uses DiceBag.Instance by default)
int value = range.Next();

// With a custom DiceBag instance
int value = range.Next(myDiceBag);
```

## Properties

| Property | Description |
|---|---|
| `Min` | Inclusive lower bound |
| `Max` | Inclusive upper bound |

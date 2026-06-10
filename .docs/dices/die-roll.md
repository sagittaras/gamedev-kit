# DieRoll

`DieRoll` is a value type that describes a dice roll configuration: a guaranteed base value plus a random
component determined by the number of die sides. The resulting value is always in the range
`[BaseValue, BaseValue + DieSides]` — the base value is the minimum guaranteed outcome.

This makes it a natural fit for any game mechanic where a value has both a guaranteed floor and a random
variance — damage, healing, resource generation, spawn counts, and similar systems.

## Creating a DieRoll

```csharp
// new DieRoll(baseValue, dieSides)
DieRoll d6      = new DieRoll(0, 6);    // result: 0–6
DieRoll d20     = new DieRoll(0, 20);   // result: 0–20
DieRoll plus5d8 = new DieRoll(5, 8);    // result: 5–13  (d8 + 5 base)

// Tuple implicit conversion
DieRoll d6 = (0, 6);
```

`dieSides` must be at least `1`. Passing `0` or a negative value throws `ArgumentOutOfRangeException`.

## Properties

| Property | Description |
|---|---|
| `BaseValue` | Guaranteed minimum contribution to the result (also the minimum outcome minus the die roll) |
| `DieSides` | Number of sides on the die — defines the random variance |
| `MaxValue` | `BaseValue + DieSides` — the highest possible outcome |

## Rolling

```csharp
IDiceBag dice = DiceBag.Instance;

DieRoll damage = new DieRoll(5, 8); // 5 + d8

// Via DiceBag
int result = dice.Roll(damage);

// Via extension method (uses DiceBag.Instance by default)
int result = damage.Roll();

// With a custom DiceBag instance
int result = damage.Roll(myDiceBag);
```

## Use Case: Spell Damage

`DieRoll` maps naturally onto spell and combat formulas common in RPG systems. A spell that deals
`5 + 1d8` damage is expressed directly:

```csharp
DieRoll frostbolt = new DieRoll(5, 8);  // 5–13 damage

int damage = frostbolt.Roll();
```

Scaling the base value at runtime is straightforward with arithmetic operators:

```csharp
DieRoll baseDamage = new DieRoll(5, 8);

// Increase base damage by spell power bonus
DieRoll scaled = baseDamage + spellPowerBonus;

// Halve base damage (e.g. partial resist)
DieRoll reduced = baseDamage / 2;
```

## Arithmetic Operators

Operators modify `BaseValue` while keeping `DieSides` unchanged:

```csharp
DieRoll d = new DieRoll(5, 8);

DieRoll a = d + 3;  // DieRoll(8, 8)  — result: 8–16
DieRoll b = d - 2;  // DieRoll(3, 8)  — result: 3–11
DieRoll c = d * 2;  // DieRoll(10, 8) — result: 10–18
DieRoll e = d / 2;  // DieRoll(2, 8)  — result: 2–10
```

Division by zero throws `ArgumentOutOfRangeException`.

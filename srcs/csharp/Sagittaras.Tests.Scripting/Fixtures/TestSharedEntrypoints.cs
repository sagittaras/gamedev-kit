using Sagittaras.Scripting;
using Sagittaras.Scripting.Attributes;

namespace Sagittaras.Tests.Scripting.Fixtures;

// --- shared entity hierarchy, plus a disjoint one for the same entity ---

[GameScript]
public abstract class LootableScript : IEntityScript
{
    public virtual int OnGoldLooted(int gold)
    {
        return gold;
    }
}

[GameScript]
public abstract class RareLootableScript : LootableScript
{
    public virtual int OnRarityRolled(int rarity)
    {
        return rarity;
    }
}

[GameScript]
public abstract class InteractableScript : IEntityScript
{
    public virtual string OnInteracted()
    {
        return "nothing";
    }
}

// --- global hierarchy ---

[GameScript]
public abstract class DifficultyScript : IGlobalScript
{
    public int AppliedCount { get; private set; }

    public virtual int OnIncomingDamage(int damage)
    {
        AppliedCount++;
        return damage;
    }

    /// <summary>
    ///     Null stands for no opinion, so the hook-point can be asked with a nullable result.
    /// </summary>
    public virtual string? OnTitleChosen()
    {
        return null;
    }

    public virtual int OnDamageCapped()
    {
        return 0;
    }
}

[GameScript]
public abstract class NightmareDifficultyScript : DifficultyScript;

/// <summary>
///     Global script deriving from no entrypoint at all - nothing could ever reach it.
/// </summary>
public class OrphanScript : IGlobalScript;
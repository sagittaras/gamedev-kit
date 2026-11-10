using Sagittaras.Scripting;
using Sagittaras.Scripting.Activation;
using Sagittaras.Scripting.Attributes;

namespace Sagittaras.Tests.Scripting.Fixtures;

// --- concrete scripts written by an extending module ---

/// <summary>
///     Reaches the whole transient chain: creature, world object and object.
/// </summary>
public class BossScript : CreatureScript
{
    public override string OnInspected()
    {
        return nameof(BossScript);
    }
}

/// <summary>
///     Sits on the middle of the chain, so it is unreachable through the creature entrypoint.
/// </summary>
public class CrateScript : WorldObjectScript
{
    public override string OnInspected()
    {
        return nameof(CrateScript);
    }
}

public class QuietScript : SilentScript;

public class ChestLootScript : RareLootableScript
{
    public override int OnGoldLooted(int gold)
    {
        return gold * 2;
    }
}

public class ChestInteractionScript : InteractableScript
{
    public override string OnInteracted()
    {
        return "opened";
    }
}

[ScriptPriority(100)]
public class NightmareScript : NightmareDifficultyScript
{
    public override int OnIncomingDamage(int damage)
    {
        base.OnIncomingDamage(damage);
        return damage * 3;
    }

    public override int OnDamageCapped()
    {
        return 0;
    }

    public override string? OnTitleChosen()
    {
        return "Nightmare";
    }
}

[ScriptPriority(10)]
public class HardScript : DifficultyScript
{
    public override int OnIncomingDamage(int damage)
    {
        base.OnIncomingDamage(damage);
        return damage * 2;
    }

    public override int OnDamageCapped()
    {
        return 42;
    }
}

public class CasualScript : DifficultyScript
{
    public override int OnDamageCapped()
    {
        return 7;
    }
}

/// <summary>
///     Activator counting what it handed out and took back, standing in for a pool.
/// </summary>
public class CountingActivator : IScriptActivator
{
    public int Created { get; private set; }
    public int Released { get; private set; }

    public TScript Create<TScript>() where TScript : class, ITransientEntityScript, new()
    {
        Created++;
        return new TScript();
    }

    public void Release(ITransientEntityScript script)
    {
        Released++;
    }
}

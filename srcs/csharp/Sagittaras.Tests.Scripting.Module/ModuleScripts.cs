using Sagittaras.Scripting;
using Sagittaras.Scripting.Attributes;

namespace Sagittaras.Tests.Scripting.Module;

[ScriptedEntity("goblin")]
public class GoblinScript : UnitScript
{
    public override string OnInspected()
    {
        return nameof(GoblinScript);
    }
}

[ScriptedEntity("goblin_boss")]
public class GoblinBossScript : EliteUnitScript
{
    public override string OnInspected()
    {
        return nameof(GoblinBossScript);
    }

    public override int OnArmorCalculated(int armor)
    {
        return armor * 2;
    }
}

[ScriptedEntity("barrel")]
public class BarrelScript : ContainerScript
{
    public override int OnGoldLooted(int gold)
    {
        return gold + 5;
    }
}

[ScriptPriority(50)]
public class ModuleRulesScript : RulesScript
{
    public override int OnIncomingDamage(int damage)
    {
        return damage * 3;
    }
}

/// <summary>
///     Serves several kinds of beast at once, each spawning its own instance of the script.
/// </summary>
[ScriptedEntity("wolf")]
[ScriptedEntity("dire_wolf")]
public class BeastScript : UnitScript
{
    public override string OnInspected()
    {
        return nameof(BeastScript);
    }
}

/// <summary>
///     Serves several kinds of container by a single shared instance.
/// </summary>
[ScriptedEntity("crate")]
[ScriptedEntity("sack")]
public class SmallContainerScript : ContainerScript
{
    public override int OnGoldLooted(int gold)
    {
        return gold + 1;
    }
}

/// <summary>
///     Implements a marker interface but derives from no entrypoint, so nothing could ever call it.
/// </summary>
/// <remarks>
///     Discovery has to walk past it rather than choke on it.
/// </remarks>
public class LooseScript : IGlobalScript;
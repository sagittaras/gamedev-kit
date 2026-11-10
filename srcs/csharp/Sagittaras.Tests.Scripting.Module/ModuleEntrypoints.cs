using Sagittaras.Scripting;
using Sagittaras.Scripting.Attributes;
using Sagittaras.Scripting.Entities;

namespace Sagittaras.Tests.Scripting.Module;

/// <summary>
///     Arguments the core offers when it spawns a unit.
/// </summary>
public readonly struct ModuleArgs(int level)
{
    public int Level { get; } = level;
}

[GameScript]
public abstract class UnitScript : ITransientEntityScript<ModuleArgs>
{
    public int Level { get; private set; }

    public void Initialize(ITransientScriptedEntity entity, in ModuleArgs args)
    {
        Level = args.Level;
    }

    public virtual string OnInspected()
    {
        return nameof(UnitScript);
    }
}

[GameScript]
public abstract class EliteUnitScript : UnitScript
{
    public virtual int OnArmorCalculated(int armor)
    {
        return armor;
    }
}

[GameScript]
public abstract class ContainerScript : IEntityScript
{
    public virtual int OnGoldLooted(int gold)
    {
        return gold;
    }
}

[GameScript]
public abstract class RulesScript : IGlobalScript
{
    public virtual int OnIncomingDamage(int damage)
    {
        return damage;
    }
}
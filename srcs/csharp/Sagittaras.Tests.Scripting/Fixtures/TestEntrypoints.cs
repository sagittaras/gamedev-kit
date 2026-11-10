using Sagittaras.Scripting;
using Sagittaras.Scripting.Attributes;
using Sagittaras.Scripting.Entities;

namespace Sagittaras.Tests.Scripting.Fixtures;

/// <summary>
///     Arguments a spawn of a scripted entity offers to its script.
/// </summary>
public readonly struct SpawnArgs(int level)
{
    public int Level { get; } = level;
}

// --- transient hierarchy: object -> world object -> creature ---

[GameScript]
public abstract class ObjectScript : ITransientEntityScript<SpawnArgs>
{
    public ITransientScriptedEntity? Entity { get; private set; }
    public int Level { get; private set; }
    public int InitializeCount { get; private set; }

    public virtual void Initialize(ITransientScriptedEntity entity, in SpawnArgs args)
    {
        Entity = entity;
        Level = args.Level;
        InitializeCount++;
    }

    public virtual string OnInspected()
    {
        return nameof(ObjectScript);
    }
}

[GameScript]
public abstract class WorldObjectScript : ObjectScript;

/// <summary>
///     Deliberately not an entrypoint - it must not show up in the resolved hierarchy.
/// </summary>
public abstract class UnmarkedScript : WorldObjectScript;

[GameScript]
public abstract class CreatureScript : UnmarkedScript
{
    public int Damage { get; private set; }

    public virtual void OnDamageTaken(int damage)
    {
        Damage += damage;
    }
}

[GameScript]
public abstract class SilentScript : ITransientEntityScript<NoArgs>
{
    public bool Initialized { get; private set; }

    public void Initialize(ITransientScriptedEntity entity, in NoArgs args)
    {
        Initialized = true;
    }
}
using AwesomeAssertions;
using Sagittaras.Scripting;
using Sagittaras.Scripting.Building;
using Sagittaras.Tests.Scripting.Fixtures;

namespace Sagittaras.Tests.Scripting.Registries;

public class ScriptLookupTest
{
    private const string ChestId = "chest";
    private const string CreatureId = "mycreature";

    private readonly TestEntity _chest = new(ChestId);
    private readonly TestEntity _barrel = new("barrel");
    private readonly TestTransientEntity _creature = new(CreatureId, 1);
    private readonly SpawnArgs _args = new(7);

    private static IScriptManager CreateManager()
    {
        return new ScriptRegistryBuilder()
            .AddGlobal(new HardScript())
            .AddGlobal(new CasualScript())
            .AddEntity(ChestId, new ChestLootScript())
            .AddTransient<BossScript>(CreatureId)
            .Build();
    }

    /// <summary>
    ///     Verify that an extended global hook-point is reported as extended.
    /// </summary>
    [Fact]
    public void HasScript_TellsWhetherAGlobalHookPointIsExtended()
    {
        CreateManager().HasScript<DifficultyScript>().Should().BeTrue();
        new ScriptRegistryBuilder().Build().HasScript<DifficultyScript>().Should().BeFalse();
    }

    /// <summary>
    ///     Verify that every script of a global hook-point is returned, in the order a call reaches them.
    /// </summary>
    [Fact]
    public void GetScripts_ReturnsEveryScriptInCallOrder()
    {
        CreateManager().GetScripts<DifficultyScript>()
            .Select(script => script.GetType())
            .Should()
            .Equal(typeof(HardScript), typeof(CasualScript));
    }

    /// <summary>
    ///     Verify that an entrypoint nobody extends yields an empty list rather than null.
    /// </summary>
    [Fact]
    public void GetScripts_ReturnsEmptyListWithoutAnyScript()
    {
        new ScriptRegistryBuilder().Build().GetScripts<DifficultyScript>().Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    ///     Verify that the script of an entity is reported and handed over.
    /// </summary>
    [Fact]
    public void GetScript_ReturnsTheScriptOfTheEntity()
    {
        IScriptManager manager = CreateManager();

        manager.HasScript<LootableScript>(_chest).Should().BeTrue();
        manager.GetScript<LootableScript>(_chest).Should().BeOfType<ChestLootScript>();
    }

    /// <summary>
    ///     Verify that an entity without a script is reported as such instead of throwing.
    /// </summary>
    [Fact]
    public void GetScript_ReturnsNullForEntityWithoutScript()
    {
        IScriptManager manager = CreateManager();

        manager.HasScript<LootableScript>(_barrel).Should().BeFalse();
        manager.GetScript<LootableScript>(_barrel).Should().BeNull();
    }

    /// <summary>
    ///     Verify that the script of an entity is reachable through an entrypoint above its own.
    /// </summary>
    [Fact]
    public void GetScript_ReachesTheScriptThroughAnAncestorEntrypoint()
    {
        CreateManager().GetScript<RareLootableScript>(_chest).Should().BeOfType<ChestLootScript>();
    }

    /// <summary>
    ///     Verify that a per-instance script is reported only while it lives.
    /// </summary>
    /// <remarks>
    ///     Nothing before the spawn, the very instance in between, nothing again once the entity is released.
    /// </remarks>
    [Fact]
    public void GetScript_FollowsTheLifecycleOfTheInstance()
    {
        IScriptManager manager = CreateManager();
        manager.HasScript<CreatureScript>(_creature).Should().BeFalse();
        manager.GetScript<CreatureScript>(_creature).Should().BeNull();

        manager.Instantiate<CreatureScript, SpawnArgs>(_creature, in _args);

        manager.HasScript<CreatureScript>(_creature).Should().BeTrue();
        manager.GetScript<CreatureScript>(_creature).Should().BeOfType<BossScript>();

        _creature.Release();

        manager.HasScript<CreatureScript>(_creature).Should().BeFalse();
        manager.GetScript<CreatureScript>(_creature).Should().BeNull();
    }

    /// <summary>
    ///     Verify that the handed over script is the same instance the calls reach.
    /// </summary>
    [Fact]
    public void GetScript_HandsOverTheInstanceTheCallsReach()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_creature, in _args);

        CreatureScript? script = manager.GetScript<CreatureScript>(_creature);
        script!.OnDamageTaken(5);

        manager.Call<CreatureScript, int>(_creature, s => s.Damage).Should().Be(5);
    }
}
using AwesomeAssertions;
using Sagittaras.Scripting.Building;
using Sagittaras.Scripting;
using Sagittaras.Tests.Scripting.Fixtures;

namespace Sagittaras.Tests.Scripting.Registries;

public class TransientLifecycleTest
{
    private const string CreatureId = "mycreature";

    private readonly TestTransientEntity _first = new(CreatureId, 1);
    private readonly TestTransientEntity _second = new(CreatureId, 2);
    private readonly SpawnArgs _args = new(7);

    private static IScriptManager CreateManager()
    {
        return new ScriptRegistryBuilder().AddTransient<BossScript>(CreatureId).Build();
    }

    /// <summary>
    ///     Verify that every instance of the same entity keeps its own state.
    /// </summary>
    [Fact]
    public void Instantiate_KeepsInstancesApart()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_first, in _args);
        manager.Instantiate<CreatureScript, SpawnArgs>(_second, in _args);

        manager.Call<CreatureScript>(_first, script => script.OnDamageTaken(5));

        manager.Call<CreatureScript, int>(_first, script => script.Damage).Should().Be(5);
        manager.Call<CreatureScript, int>(_second, script => script.Damage).Should().Be(0);
    }

    /// <summary>
    ///     Verify that an instance number is read together with the entity it belongs to.
    /// </summary>
    /// <remarks>
    ///     Instance numbers are unique only within an entity identifier, so two different creatures may both
    ///     be the first of their kind without sharing a script.
    /// </remarks>
    [Fact]
    public void Instantiate_KeepsInstanceNumbersOfDifferentEntitiesApart()
    {
        IScriptManager manager = new ScriptRegistryBuilder()
            .AddTransient<BossScript>(CreatureId)
            .AddTransient<CrateScript>("mycrate")
            .Build();

        TestTransientEntity crate = new("mycrate", 1);
        manager.Instantiate<CreatureScript, SpawnArgs>(_first, in _args);
        manager.Instantiate<WorldObjectScript, SpawnArgs>(crate, in _args);

        manager.Call<WorldObjectScript, string>(_first, script => script.OnInspected()).Should().Be(nameof(BossScript));
        manager.Call<WorldObjectScript, string>(crate, script => script.OnInspected()).Should().Be(nameof(CrateScript));
    }

    /// <summary>
    ///     Verify that releasing the entity drops its script, through every entrypoint at once.
    /// </summary>
    [Fact]
    public void Released_DropsScriptOfTheEntity()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_first, in _args);

        _first.Release();

        manager.Call<CreatureScript, int>(_first, script => script.Level).Should().Be(0);
        manager.Call<ObjectScript, int>(_first, script => script.Level).Should().Be(0);
    }

    /// <summary>
    ///     Verify that releasing one entity leaves the scripts of the others alone.
    /// </summary>
    [Fact]
    public void Released_LeavesOtherInstancesAlone()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_first, in _args);
        manager.Instantiate<CreatureScript, SpawnArgs>(_second, in _args);

        _first.Release();

        manager.Call<CreatureScript, int>(_second, script => script.Level).Should().Be(7);
    }

    /// <summary>
    ///     Verify that the registry stops listening to an entity it no longer holds a script for.
    /// </summary>
    /// <remarks>
    ///     A pooled entity outlives its script, so a handler left behind would keep the registry reachable
    ///     from it and pile up on every further spawn.
    /// </remarks>
    [Fact]
    public void Released_UnsubscribesFromTheEntity()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_first, in _args);
        _first.SubscriberCount.Should().Be(1);

        _first.Release();

        _first.SubscriberCount.Should().Be(0);
    }

    /// <summary>
    ///     Verify that a pooled entity coming back to life is served by a freshly initialized script.
    /// </summary>
    /// <remarks>
    ///     The instance itself is not new - a manager pools the transient scripts unless told otherwise - so
    ///     what a respawn is owed is the initialization, not the allocation.
    /// </remarks>
    [Fact]
    public void Instantiate_ReinitializesAfterRelease()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_first, in _args);
        _first.Release();

        SpawnArgs respawn = new(3);
        manager.Instantiate<CreatureScript, SpawnArgs>(_first, in respawn);

        manager.Call<CreatureScript, int>(_first, script => script.Level).Should().Be(3);
        manager.Call<CreatureScript, object?>(_first, script => script.Entity).Should().BeSameAs(_first);
        _first.SubscriberCount.Should().Be(1);
    }

    /// <summary>
    ///     Verify that releasing an entity that holds no script does nothing.
    /// </summary>
    [Fact]
    public void Released_IgnoresEntityWithoutScript()
    {
        CreateManager();

        _first.Invoking(entity => entity.Release()).Should().NotThrow();
    }
}
using AwesomeAssertions;
using Sagittaras.Scripting.Building;
using Sagittaras.Scripting;
using Sagittaras.Scripting.Entities;
using Sagittaras.Tests.Scripting.Fixtures;

namespace Sagittaras.Tests.Scripting.Registries;

public class EntityScriptRegistryTest
{
    private const string ChestId = "chest";

    private readonly TestEntity _chest = new(ChestId);
    private readonly TestEntity _barrel = new("barrel");

    /// <summary>
    ///     Creates the manager where the chest is both lootable and interactive.
    /// </summary>
    private IScriptManager CreateManager()
    {
        return new ScriptRegistryBuilder()
            .AddEntity(ChestId, new ChestLootScript())
            .AddEntity(ChestId, new ChestInteractionScript())
            .Build();
    }

    /// <summary>
    ///     Verify that the script of an entity answers a hook-point of its own entrypoint.
    /// </summary>
    [Fact]
    public void Call_ReachesScriptOfTheEntity()
    {
        CreateManager().Call<RareLootableScript, int>(_chest, script => script.OnGoldLooted(10)).Should().Be(20);
    }

    /// <summary>
    ///     Verify that the script is reachable through an entrypoint above the one it derives from.
    /// </summary>
    [Fact]
    public void Call_ReachesScriptThroughAncestorEntrypoint()
    {
        CreateManager().Call<LootableScript, int>(_chest, script => script.OnGoldLooted(10)).Should().Be(20);
    }

    /// <summary>
    ///     Verify that an entity may be served by scripts of hierarchies that never meet.
    /// </summary>
    [Fact]
    public void Call_ReachesScriptsOfDisjointHierarchies()
    {
        IScriptManager manager = CreateManager();

        manager.Call<LootableScript, int>(_chest, script => script.OnGoldLooted(10)).Should().Be(20);
        manager.Call<InteractableScript, string>(_chest, script => script.OnInteracted()).Should().Be("opened");
    }

    /// <summary>
    ///     Verify that an entity without a script keeps the default behavior of the core module.
    /// </summary>
    [Fact]
    public void Call_DoesNothingForEntityWithoutScript()
    {
        CreateManager().Call<LootableScript, int>(_barrel, script => script.OnGoldLooted(10)).Should().Be(0);
    }

    /// <summary>
    ///     Verify that two scripts answering the same hook-point of the same entity are refused.
    /// </summary>
    [Fact]
    public void AddEntity_RefusesSecondScriptOfTheSameEntrypoint()
    {
        new ScriptRegistryBuilder()
            .AddEntity(ChestId, new ChestLootScript())
            .Invoking(builder => builder.AddEntity(ChestId, new ChestLootScript()))
            .Should()
            .Throw<ArgumentException>();
    }

    /// <summary>
    ///     Verify that the collision is detected on a shared ancestor, not only on the declared entrypoint.
    /// </summary>
    /// <remarks>
    ///     A rare lootable script and a plain lootable one meet at <see cref="LootableScript"/>, where a call
    ///     could no longer tell which of them should answer.
    /// </remarks>
    [Fact]
    public void AddEntity_RefusesCollisionOnSharedAncestor()
    {
        new ScriptRegistryBuilder()
            .AddEntity(ChestId, new ChestLootScript())
            .Invoking(builder => builder.AddEntity(ChestId, new PlainLootScript()))
            .Should()
            .Throw<ArgumentException>();
    }

    /// <summary>
    ///     Verify that a refused registration leaves the builder untouched.
    /// </summary>
    [Fact]
    public void AddEntity_RefusedRegistrationChangesNothing()
    {
        ScriptRegistryBuilder builder = new();
        builder.AddEntity(ChestId, new ChestLootScript());
        builder.Invoking(b => b.AddEntity(ChestId, new PlainLootScript())).Should().Throw<ArgumentException>();

        builder.Build().Call<LootableScript, int>(_chest, script => script.OnGoldLooted(10)).Should().Be(20);
    }
}

/// <summary>
///     Lootable script that is not rare, so it collides with the chest one on their shared ancestor.
/// </summary>
public class PlainLootScript : LootableScript
{
    public override int OnGoldLooted(int gold)
    {
        return gold + 1;
    }
}
using System.Reflection;
using Sagittaras.Scripting.Building;
using AwesomeAssertions;
using Sagittaras.Scripting;
using Sagittaras.Scripting.Discovery;
using Sagittaras.Tests.Scripting.Fixtures;
using Sagittaras.Tests.Scripting.Module;

namespace Sagittaras.Tests.Scripting.Discovery;

public class ScriptDiscoveryRegistrationTest
{
    private static readonly Assembly ModuleAssembly = typeof(GoblinScript).Assembly;

    private readonly TestTransientEntity _goblin = new("goblin", 1);
    private readonly TestTransientEntity _boss = new("goblin_boss", 1);
    private readonly TestEntity _barrel = new("barrel");
    private readonly ModuleArgs _args = new(3);

    /// <summary>
    ///     Boots the game the way a core module would - scan the module, register, done.
    /// </summary>
    private static IScriptManager Boot()
    {
        return new ScriptRegistryBuilder().Discover(ModuleAssembly).Build();
    }

    /// <summary>
    ///     Verify that a discovered transient script is created and reached on a spawn.
    /// </summary>
    [Fact]
    public void Discover_RegistersTransientScripts()
    {
        IScriptManager manager = Boot();
        manager.Instantiate<UnitScript, ModuleArgs>(_goblin, in _args);

        manager.Call<UnitScript, string>(_goblin, script => script.OnInspected()).Should().Be(nameof(GoblinScript));
        manager.Call<UnitScript, int>(_goblin, script => script.Level).Should().Be(3);
    }

    /// <summary>
    ///     Verify that a discovered script keeps the whole chain of its entrypoints.
    /// </summary>
    [Fact]
    public void Discover_KeepsTheHierarchyOfDiscoveredScripts()
    {
        IScriptManager manager = Boot();
        manager.Instantiate<EliteUnitScript, ModuleArgs>(_boss, in _args);

        manager.Call<EliteUnitScript, int>(_boss, script => script.OnArmorCalculated(10)).Should().Be(20);
        manager.Call<UnitScript, string>(_boss, script => script.OnInspected()).Should().Be(nameof(GoblinBossScript));
    }

    /// <summary>
    ///     Verify that a discovered shared entity script answers for its entity.
    /// </summary>
    [Fact]
    public void Discover_RegistersEntityScripts()
    {
        Boot().Call<ContainerScript, int>(_barrel, script => script.OnGoldLooted(10)).Should().Be(15);
    }

    /// <summary>
    ///     Verify that a discovered global script answers a hook-point without an entity target.
    /// </summary>
    [Fact]
    public void Discover_RegistersGlobalScripts()
    {
        Boot().Call<RulesScript, int>(script => script.OnIncomingDamage(2)).Should().Be(6);
    }

    /// <summary>
    ///     Verify that registering the same script twice reaches the caller as the refusal of the builder.
    /// </summary>
    /// <remarks>
    ///     Transient scripts are registered through reflection, which would otherwise bury the reason inside
    ///     a <see cref="TargetInvocationException"/>.
    /// </remarks>
    [Fact]
    public void Discover_ReportsCollisionWithoutTheReflectionWrapper()
    {
        ScriptRegistryBuilder builder = new();
        builder.Discover(ModuleAssembly);

        builder.Invoking(b => b.Discover(ModuleAssembly))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("*goblin*");
    }

    /// <summary>
    ///     Verify that a transient script naming several entities is spawned for each of them.
    /// </summary>
    [Fact]
    public void Discover_RegistersTransientScriptForEveryNamedEntity()
    {
        IScriptManager manager = Boot();
        TestTransientEntity wolf = new("wolf", 1);
        TestTransientEntity direWolf = new("dire_wolf", 1);

        manager.Instantiate<UnitScript, ModuleArgs>(wolf, in _args);
        manager.Instantiate<UnitScript, ModuleArgs>(direWolf, in _args);

        manager.Call<UnitScript, string>(wolf, script => script.OnInspected()).Should().Be(nameof(BeastScript));
        manager.Call<UnitScript, string>(direWolf, script => script.OnInspected()).Should().Be(nameof(BeastScript));
    }

    /// <summary>
    ///     Verify that every entity of a transient script still gets an instance of its own.
    /// </summary>
    [Fact]
    public void Discover_KeepsInstancesOfSharedTransientScriptApart()
    {
        IScriptManager manager = Boot();
        TestTransientEntity wolf = new("wolf", 1);
        TestTransientEntity direWolf = new("dire_wolf", 1);
        ModuleArgs stronger = new(9);

        manager.Instantiate<UnitScript, ModuleArgs>(wolf, in _args);
        manager.Instantiate<UnitScript, ModuleArgs>(direWolf, in stronger);

        manager.Call<UnitScript, int>(wolf, script => script.Level).Should().Be(3);
        manager.Call<UnitScript, int>(direWolf, script => script.Level).Should().Be(9);
    }

    /// <summary>
    ///     Verify that a shared entity script naming several entities answers for each of them.
    /// </summary>
    [Fact]
    public void Discover_RegistersEntityScriptForEveryNamedEntity()
    {
        IScriptManager manager = Boot();

        manager.Call<ContainerScript, int>(new TestEntity("crate"), script => script.OnGoldLooted(10)).Should().Be(11);
        manager.Call<ContainerScript, int>(new TestEntity("sack"), script => script.OnGoldLooted(10)).Should().Be(11);
    }

    /// <summary>
    ///     Verify that the entities of a shared script are served by one and the same instance.
    /// </summary>
    /// <remarks>
    ///     A shared script holds no state of an entity, so creating one per named entity would only be waste.
    /// </remarks>
    [Fact]
    public void Discover_SharesOneInstanceAcrossNamedEntities()
    {
        IScriptManager manager = Boot();

        ContainerScript? crate = manager.Call<ContainerScript, ContainerScript>(new TestEntity("crate"), script => script);
        ContainerScript? sack = manager.Call<ContainerScript, ContainerScript>(new TestEntity("sack"), script => script);

        crate.Should().NotBeNull().And.BeSameAs(sack);
    }
}
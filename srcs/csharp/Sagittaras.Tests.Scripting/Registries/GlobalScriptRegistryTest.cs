using AwesomeAssertions;
using Sagittaras.Scripting.Building;
using Sagittaras.Scripting;
using Sagittaras.Tests.Scripting.Fixtures;

namespace Sagittaras.Tests.Scripting.Registries;

public class GlobalScriptRegistryTest
{
    private readonly NightmareScript _nightmare = new();
    private readonly HardScript _hard = new();
    private readonly CasualScript _casual = new();

    /// <summary>
    ///     Creates the manager over all three difficulty scripts.
    /// </summary>
    private IScriptManager CreateManager()
    {
        return new ScriptRegistryBuilder()
            .AddGlobal(_casual)
            .AddGlobal(_hard)
            .AddGlobal(_nightmare)
            .Build();
    }

    /// <summary>
    ///     Verify that a global hook-point is a broadcast reaching every registered script.
    /// </summary>
    [Fact]
    public void Call_ReachesEveryRegisteredScript()
    {
        CreateManager().Call<DifficultyScript>(script => script.OnIncomingDamage(1));

        _casual.AppliedCount.Should().Be(1);
        _hard.AppliedCount.Should().Be(1);
        _nightmare.AppliedCount.Should().Be(1);
    }

    /// <summary>
    ///     Verify that scripts are ordered by their declared priority, the highest first.
    /// </summary>
    [Fact]
    public void Call_OrdersScriptsByPriority()
    {
        List<string> called = [];
        CreateManager().Call<DifficultyScript>(script => called.Add(script.GetType().Name));

        called.Should().Equal(nameof(NightmareScript), nameof(HardScript), nameof(CasualScript));
    }

    /// <summary>
    ///     Verify that a script is reachable through the entrypoint it declares, not only through the root one.
    /// </summary>
    [Fact]
    public void Call_ReachesScriptThroughItsOwnEntrypoint()
    {
        List<string> called = [];
        CreateManager().Call<NightmareDifficultyScript>(script => called.Add(script.GetType().Name));

        called.Should().Equal(nameof(NightmareScript));
    }

    /// <summary>
    ///     Verify that the first script answering a hook-point wins and the rest are not asked.
    /// </summary>
    [Fact]
    public void Call_ReturnsFirstNonDefaultResult()
    {
        CreateManager().Call<DifficultyScript, int>(script => script.OnDamageCapped()).Should().Be(42);
    }

    /// <summary>
    ///     Verify that a hook-point nobody extends is an ordinary state rather than an error.
    /// </summary>
    [Fact]
    public void Call_DoesNothingWithoutRegisteredScript()
    {
        IScriptManager manager = new ScriptRegistryBuilder().Build();

        manager.Invoking(m => m.Call<DifficultyScript>(script => script.OnIncomingDamage(1))).Should().NotThrow();
        manager.Call<DifficultyScript, int>(script => script.OnDamageCapped()).Should().Be(0);
    }

    /// <summary>
    ///     Verify that a script no entrypoint could ever reach is refused at registration.
    /// </summary>
    [Fact]
    public void AddGlobal_RefusesScriptWithoutEntrypoint()
    {
        new ScriptRegistryBuilder()
            .Invoking(builder => builder.AddGlobal(new OrphanScript()))
            .Should()
            .Throw<ArgumentException>();
    }
}
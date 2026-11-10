using AwesomeAssertions;
using Sagittaras.Scripting;
using Sagittaras.Scripting.Building;
using Sagittaras.Tests.Scripting.Fixtures;

namespace Sagittaras.Tests.Scripting.Registries;

public class CallDefaultTest
{
    private const string ChestId = "chest";
    private const string CreatureId = "mycreature";

    private readonly TestEntity _chest = new(ChestId);
    private readonly TestEntity _barrel = new("barrel");
    private readonly TestTransientEntity _creature = new(CreatureId, 1);
    private readonly SpawnArgs _args = new(7);

    /// <summary>
    ///     Verify that a global hook-point nobody extends falls back to the value the call site supplied.
    /// </summary>
    [Fact]
    public void Call_ReturnsSuppliedDefaultWithoutAnyScript()
    {
        IScriptManager manager = new ScriptRegistryBuilder().Build();

        manager.Call<DifficultyScript, int>(script => script.OnDamageCapped(), 99).Should().Be(99);
    }

    /// <summary>
    ///     Verify that what counts as no opinion stays tied to the default of the result type.
    /// </summary>
    /// <remarks>
    ///     A script never sees the value the call site supplied - the hook is handed nothing but the script
    ///     itself - so it could not deliberately return it to pass the question on. Answering is therefore
    ///     measured against the default of the type, and the supplied value only decides what comes back
    ///     when nobody answered at all.
    /// </remarks>
    [Fact]
    public void Call_MeasuresNoOpinionAgainstTheDefaultOfTheType()
    {
        IScriptManager manager = new ScriptRegistryBuilder()
            .AddGlobal(new HardScript())      // caps at 42
            .AddGlobal(new CasualScript())    // caps at 7
            .Build();

        manager.Call<DifficultyScript, int>(script => script.OnDamageCapped()).Should().Be(42);
        manager.Call<DifficultyScript, int>(script => script.OnDamageCapped(), 99).Should().Be(42);
    }

    /// <summary>
    ///     Verify that the default of the type still ends the search when nothing is supplied.
    /// </summary>
    [Fact]
    public void Call_KeepsTheDefaultOfTheTypeWhenNothingIsSupplied()
    {
        IScriptManager manager = new ScriptRegistryBuilder()
            .AddGlobal(new NightmareScript())   // caps at 0, meaning no opinion
            .AddGlobal(new CasualScript())      // caps at 7
            .Build();

        manager.Call<DifficultyScript, int>(script => script.OnDamageCapped()).Should().Be(7);
    }

    /// <summary>
    ///     Verify that an entity without a script falls back to the supplied value.
    /// </summary>
    [Fact]
    public void Call_ReturnsSuppliedDefaultForEntityWithoutScript()
    {
        IScriptManager manager = new ScriptRegistryBuilder().AddEntity(ChestId, new ChestLootScript()).Build();

        manager.Call<LootableScript, int>(_barrel, script => script.OnGoldLooted(10), -1).Should().Be(-1);
        manager.Call<LootableScript, int>(_chest, script => script.OnGoldLooted(10), -1).Should().Be(20);
    }

    /// <summary>
    ///     Verify that the answer of a shared script is final, even when it is the default of the type.
    /// </summary>
    /// <remarks>
    ///     There is only one script per entrypoint here, so there is nobody else to ask. Overriding its
    ///     answer by the supplied value would hide what it actually said.
    /// </remarks>
    [Fact]
    public void Call_KeepsTheAnswerOfASharedScriptOverTheSuppliedDefault()
    {
        IScriptManager manager = new ScriptRegistryBuilder().AddEntity(ChestId, new ChestLootScript()).Build();

        manager.Call<LootableScript, int>(_chest, script => script.OnGoldLooted(0), -1).Should().Be(0);
    }

    /// <summary>
    ///     Verify that an entity instance without a script falls back to the supplied value.
    /// </summary>
    [Fact]
    public void Call_ReturnsSuppliedDefaultForInstanceWithoutScript()
    {
        IScriptManager manager = new ScriptRegistryBuilder().AddTransient<BossScript>(CreatureId).Build();

        manager.Call<CreatureScript, int>(_creature, script => script.Damage, -1).Should().Be(-1);

        manager.Instantiate<CreatureScript, SpawnArgs>(_creature, in _args);
        manager.Call<CreatureScript, int>(_creature, script => script.Damage, -1).Should().Be(0);
    }

    /// <summary>
    ///     Verify that a hook-point answering with a reference type may treat null as no opinion.
    /// </summary>
    [Fact]
    public void Call_SupportsNullableResult()
    {
        IScriptManager manager = new ScriptRegistryBuilder().AddGlobal(new CasualScript()).Build();

        manager.Call<DifficultyScript, string?>(script => script.OnTitleChosen()).Should().BeNull();
    }

    /// <summary>
    ///     Verify that a nullable result falls back to the value the call site supplied.
    /// </summary>
    [Fact]
    public void Call_ReturnsSuppliedDefaultForNullableResult()
    {
        IScriptManager manager = new ScriptRegistryBuilder().AddGlobal(new CasualScript()).Build();

        manager.Call<DifficultyScript, string?>(script => script.OnTitleChosen(), "Normal").Should().Be("Normal");
    }

    /// <summary>
    ///     Verify that a script answering wins over the supplied value, nullable result or not.
    /// </summary>
    [Fact]
    public void Call_PrefersTheAnsweringScriptOverTheNullableDefault()
    {
        IScriptManager manager = new ScriptRegistryBuilder()
            .AddGlobal(new NightmareScript())
            .AddGlobal(new CasualScript())
            .Build();

        manager.Call<DifficultyScript, string?>(script => script.OnTitleChosen(), "Normal").Should().Be("Nightmare");
    }
}
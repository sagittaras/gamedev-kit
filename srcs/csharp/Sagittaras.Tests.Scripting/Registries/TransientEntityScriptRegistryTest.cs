using AwesomeAssertions;
using Sagittaras.Scripting.Building;
using Sagittaras.Scripting;
using Sagittaras.Tests.Scripting.Fixtures;

namespace Sagittaras.Tests.Scripting.Registries;

public class TransientEntityScriptRegistryTest
{
    private const string CreatureId = "mycreature";

    private readonly TestTransientEntity _first = new(CreatureId, 1);
    private readonly TestTransientEntity _second = new(CreatureId, 2);
    private readonly SpawnArgs _args = new(7);

    /// <summary>
    ///     Creates the manager where the creature is served by a per-instance boss script.
    /// </summary>
    private static IScriptManager CreateManager()
    {
        return new ScriptRegistryBuilder()
            .AddTransient<BossScript>(CreatureId)
            .Build();
    }

    /// <summary>
    ///     Verify that the spawn creates the script and hands it the arguments of the entrypoint.
    /// </summary>
    [Fact]
    public void Instantiate_InitializesScriptWithSpawnArguments()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_first, in _args);

        manager.Call<CreatureScript, int>(_first, script => script.Level).Should().Be(7);
        manager.Call<CreatureScript, int>(_first, script => script.InitializeCount).Should().Be(1);
    }

    /// <summary>
    ///     Verify that the created script knows the very entity instance it serves.
    /// </summary>
    [Fact]
    public void Instantiate_BindsScriptToItsEntityInstance()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_first, in _args);

        manager.Call<CreatureScript, object?>(_first, script => script.Entity).Should().BeSameAs(_first);
    }

    /// <summary>
    ///     Verify that the created script is reachable through every entrypoint above it.
    /// </summary>
    [Fact]
    public void Instantiate_IndexesScriptUnderWholeChain()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_first, in _args);

        manager.Call<CreatureScript, string>(_first, script => script.OnInspected()).Should().Be(nameof(BossScript));
        manager.Call<WorldObjectScript, string>(_first, script => script.OnInspected()).Should().Be(nameof(BossScript));
        manager.Call<ObjectScript, string>(_first, script => script.OnInspected()).Should().Be(nameof(BossScript));
    }

    /// <summary>
    ///     Verify that an entrypoint offering no arguments spawns through the shorter overload.
    /// </summary>
    [Fact]
    public void Instantiate_SupportsEntrypointWithoutArguments()
    {
        IScriptManager manager = new ScriptRegistryBuilder().AddTransient<QuietScript>(CreatureId).Build();
        manager.Instantiate<SilentScript>(_first);

        manager.Call<SilentScript, bool>(_first, script => script.Initialized).Should().BeTrue();
    }

    /// <summary>
    ///     Verify that an entity with no transient script registered spawns nothing.
    /// </summary>
    [Fact]
    public void Instantiate_DoesNothingForUnregisteredEntity()
    {
        IScriptManager manager = CreateManager();
        TestTransientEntity stranger = new("other", 1);

        manager.Invoking(m => m.Instantiate<CreatureScript, SpawnArgs>(stranger, in _args)).Should().NotThrow();
        manager.Call<CreatureScript, int>(stranger, script => script.Level).Should().Be(0);
    }

    /// <summary>
    ///     Verify that spawning the same entrypoint twice for one instance is reported instead of silently replacing.
    /// </summary>
    [Fact]
    public void Instantiate_RefusesSecondScriptOfTheSameInstance()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_first, in _args);

        manager.Invoking(m => m.Instantiate<CreatureScript, SpawnArgs>(_first, in _args))
            .Should()
            .Throw<InvalidOperationException>();
    }
}
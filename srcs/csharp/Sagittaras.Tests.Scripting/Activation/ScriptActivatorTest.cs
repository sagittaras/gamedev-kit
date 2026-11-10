using AwesomeAssertions;
using Sagittaras.Scripting.Building;
using Sagittaras.Scripting;
using Sagittaras.Tests.Scripting.Fixtures;

namespace Sagittaras.Tests.Scripting.Activation;

public class ScriptActivatorTest
{
    private const string CreatureId = "mycreature";

    private readonly CountingActivator _activator = new();
    private readonly TestTransientEntity _entity = new(CreatureId, 1);
    private readonly SpawnArgs _args = new(7);

    private IScriptManager CreateManager()
    {
        return new ScriptRegistryBuilder()
            .AddTransient<BossScript>(CreatureId)
            .WithActivator(_activator)
            .Build();
    }

    /// <summary>
    ///     Verify that the transient instances come from the registered activator.
    /// </summary>
    [Fact]
    public void Instantiate_TakesInstanceFromTheActivator()
    {
        CreateManager().Instantiate<CreatureScript, SpawnArgs>(_entity, in _args);

        _activator.Created.Should().Be(1);
    }

    /// <summary>
    ///     Verify that a released script is handed back, so a pooling activator can keep it.
    /// </summary>
    [Fact]
    public void Released_HandsScriptBackToTheActivator()
    {
        CreateManager().Instantiate<CreatureScript, SpawnArgs>(_entity, in _args);

        _entity.Release();

        _activator.Released.Should().Be(1);
    }

    /// <summary>
    ///     Verify that a script is handed back once, however many entrypoints it is indexed under.
    /// </summary>
    /// <remarks>
    ///     A boss script is reachable through three entrypoints, and a pool given the same instance three
    ///     times over would hand it out to three different entities at once.
    /// </remarks>
    [Fact]
    public void Released_HandsScriptBackOnlyOnce()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_entity, in _args);
        manager.Call<ObjectScript, string>(_entity, script => script.OnInspected()).Should().Be(nameof(BossScript));

        _entity.Release();

        _activator.Released.Should().Be(1);
    }

    /// <summary>
    ///     Verify that releasing several entities hands back each of their scripts.
    /// </summary>
    [Fact]
    public void Released_HandsBackScriptOfEveryInstance()
    {
        IScriptManager manager = CreateManager();
        TestTransientEntity second = new(CreatureId, 2);
        manager.Instantiate<CreatureScript, SpawnArgs>(_entity, in _args);
        manager.Instantiate<CreatureScript, SpawnArgs>(second, in _args);

        _entity.Release();
        second.Release();

        _activator.Created.Should().Be(2);
        _activator.Released.Should().Be(2);
    }

    /// <summary>
    ///     Verify that a manager pools the transient scripts unless it is told otherwise.
    /// </summary>
    /// <remarks>
    ///     Spawning after a release counts a second initialization of the same instance, which no activator
    ///     handing out a fresh script could ever show.
    /// </remarks>
    [Fact]
    public void Build_PoolsWithoutARegisteredActivator()
    {
        IScriptManager manager = new ScriptRegistryBuilder().AddTransient<BossScript>(CreatureId).Build();

        manager.Instantiate<CreatureScript, SpawnArgs>(_entity, in _args);
        _entity.Release();
        manager.Instantiate<CreatureScript, SpawnArgs>(_entity, in _args);

        manager.Call<CreatureScript, int>(_entity, script => script.InitializeCount).Should().Be(2);
    }
}
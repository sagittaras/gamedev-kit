using AwesomeAssertions;
using Sagittaras.Scripting.Activation;
using Sagittaras.Scripting.Building;
using Sagittaras.Scripting;
using Sagittaras.Tests.Scripting.Fixtures;

namespace Sagittaras.Tests.Scripting.Activation;

public class PooledSpawnTest
{
    private const string CreatureId = "mycreature";

    private readonly PoolingScriptActivator _activator = new();
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
    ///     Verify that a released entity leaves its script waiting in the pool.
    /// </summary>
    [Fact]
    public void Released_ReturnsScriptToThePool()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_entity, in _args);
        _activator.Available(typeof(BossScript)).Should().Be(0);

        _entity.Release();

        _activator.Available(typeof(BossScript)).Should().Be(1);
    }

    /// <summary>
    ///     Verify that the next spawn takes the pooled script rather than allocating one.
    /// </summary>
    /// <remarks>
    ///     The script counts its initializations, so a second one proves the very same instance came back.
    /// </remarks>
    [Fact]
    public void Instantiate_ReusesThePooledScript()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_entity, in _args);
        _entity.Release();

        SpawnArgs respawn = new(3);
        manager.Instantiate<CreatureScript, SpawnArgs>(_entity, in respawn);

        manager.Call<CreatureScript, int>(_entity, script => script.InitializeCount).Should().Be(2);
        manager.Call<CreatureScript, int>(_entity, script => script.Level).Should().Be(3);
        _activator.Available(typeof(BossScript)).Should().Be(0);
    }

    /// <summary>
    ///     Verify that a pooled script serves a different entity instance just as well.
    /// </summary>
    [Fact]
    public void Instantiate_ReusesThePooledScriptForAnotherEntity()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_entity, in _args);
        _entity.Release();

        TestTransientEntity next = new(CreatureId, 2);
        manager.Instantiate<CreatureScript, SpawnArgs>(next, in _args);

        manager.Call<CreatureScript, object?>(next, script => script.Entity).Should().BeSameAs(next);
    }

    /// <summary>
    ///     Verify that state the script leaves untouched in the initialization survives into the next entity.
    /// </summary>
    /// <remarks>
    ///     This is the price of pooling, not a defect of it. The fixture adds up damage and never clears it,
    ///     so the damage of the previous entity comes back with the instance - which is exactly why
    ///     <see cref="ITransientEntityScript{TArgs}.Initialize"/> is contracted to overwrite everything.
    /// </remarks>
    [Fact]
    public void Instantiate_CarriesOverStateTheScriptDoesNotOverwrite()
    {
        IScriptManager manager = CreateManager();
        manager.Instantiate<CreatureScript, SpawnArgs>(_entity, in _args);
        manager.Call<CreatureScript>(_entity, script => script.OnDamageTaken(5));
        _entity.Release();

        TestTransientEntity next = new(CreatureId, 2);
        manager.Instantiate<CreatureScript, SpawnArgs>(next, in _args);

        manager.Call<CreatureScript, int>(next, script => script.Damage).Should().Be(5);
    }

    /// <summary>
    ///     Verify that scripts are told apart by their identity, not by what they call equal.
    /// </summary>
    /// <remarks>
    ///     A script comparing every instance equal would otherwise look to the pool like one instance
    ///     released twice.
    /// </remarks>
    [Fact]
    public void Release_TellsScriptsApartByReference()
    {
        _activator.Release(new EqualScript());

        _activator.Invoking(activator => activator.Release(new EqualScript())).Should().NotThrow();
        _activator.Available(typeof(EqualScript)).Should().Be(2);
    }
}

/// <summary>
///     Script calling every instance of itself equal.
/// </summary>
public class EqualScript : CreatureScript
{
    public override bool Equals(object? obj)
    {
        return obj is EqualScript;
    }

    public override int GetHashCode()
    {
        return 0;
    }
}
using AwesomeAssertions;
using Sagittaras.Scripting.Activation;
using Sagittaras.Scripting;
using Sagittaras.Tests.Scripting.Fixtures;

namespace Sagittaras.Tests.Scripting.Activation;

public class PoolingScriptActivatorTest
{
    private readonly PoolingScriptActivator _activator = new();

    /// <summary>
    ///     Verify that an empty pool still hands out a script.
    /// </summary>
    [Fact]
    public void Create_AllocatesWhenThePoolIsEmpty()
    {
        _activator.Create<BossScript>().Should().NotBeNull();
    }

    /// <summary>
    ///     Verify that a released script is handed out again instead of a fresh one.
    /// </summary>
    [Fact]
    public void Create_HandsOutTheReleasedScript()
    {
        BossScript released = _activator.Create<BossScript>();
        _activator.Release(released);

        _activator.Create<BossScript>().Should().BeSameAs(released);
    }

    /// <summary>
    ///     Verify that a script handed out is no longer waiting in the pool.
    /// </summary>
    /// <remarks>
    ///     Two entities holding the same instance is the one failure a pool must never allow.
    /// </remarks>
    [Fact]
    public void Create_HandsOutEachScriptOnlyOnce()
    {
        _activator.Release(_activator.Create<BossScript>());

        BossScript first = _activator.Create<BossScript>();
        BossScript second = _activator.Create<BossScript>();

        second.Should().NotBeSameAs(first);
    }

    /// <summary>
    ///     Verify that the pools of different scripts do not mix.
    /// </summary>
    [Fact]
    public void Create_KeepsPoolsOfDifferentScriptsApart()
    {
        _activator.Release(_activator.Create<BossScript>());

        _activator.Available(typeof(BossScript)).Should().Be(1);
        _activator.Available(typeof(CrateScript)).Should().Be(0);
        _activator.Create<CrateScript>().Should().BeOfType<CrateScript>();
    }

    /// <summary>
    ///     Verify that releasing one script twice is reported rather than pooled twice.
    /// </summary>
    [Fact]
    public void Release_ReportsScriptReleasedTwice()
    {
        BossScript script = _activator.Create<BossScript>();
        _activator.Release(script);

        _activator.Invoking(activator => activator.Release(script))
            .Should()
            .Throw<InvalidOperationException>();
    }

    /// <summary>
    ///     Verify that a pool at its capacity drops the rest instead of growing.
    /// </summary>
    [Fact]
    public void Release_StopsAtTheCapacity()
    {
        PoolingScriptActivator activator = new(2);
        for (int i = 0; i < 5; i++)
        {
            activator.Release(new BossScript());
        }

        activator.Available(typeof(BossScript)).Should().Be(2);
    }

    /// <summary>
    ///     Verify that prewarming fills the pool before anything spawns.
    /// </summary>
    [Fact]
    public void Prewarm_FillsThePoolInAdvance()
    {
        _activator.Prewarm<BossScript>(3);

        _activator.Available(typeof(BossScript)).Should().Be(3);
    }

    /// <summary>
    ///     Verify that clearing drops the held scripts.
    /// </summary>
    [Fact]
    public void Clear_DropsHeldScripts()
    {
        _activator.Prewarm<BossScript>(3);

        _activator.Clear();

        _activator.Available(typeof(BossScript)).Should().Be(0);
    }

    /// <summary>
    ///     Verify that a negative capacity is refused.
    /// </summary>
    [Fact]
    public void Constructor_RefusesNegativeCapacity()
    {
        FluentActions.Invoking(() => new PoolingScriptActivator(-1))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    ///     Verify that a capacity of zero keeps nothing, so that every spawn allocates again.
    /// </summary>
    /// <remarks>
    ///     The way to take pooling out of the picture while telling a state left over from the previous
    ///     entity apart from a defect of the script itself.
    /// </remarks>
    [Fact]
    public void Release_KeepsNothingAtZeroCapacity()
    {
        PoolingScriptActivator activator = new(0);
        BossScript script = activator.Create<BossScript>();

        activator.Release(script);

        activator.Available(typeof(BossScript)).Should().Be(0);
        activator.Create<BossScript>().Should().NotBeSameAs(script);
    }

    /// <summary>
    ///     Verify that an unbounded pool keeps every script released to it.
    /// </summary>
    [Fact]
    public void Release_KeepsEverythingWhenUnbounded()
    {
        PoolingScriptActivator activator = new(PoolingScriptActivator.Unbounded);
        for (int i = 0; i < 50; i++)
        {
            activator.Release(new BossScript());
        }

        activator.Available(typeof(BossScript)).Should().Be(50);
    }
}
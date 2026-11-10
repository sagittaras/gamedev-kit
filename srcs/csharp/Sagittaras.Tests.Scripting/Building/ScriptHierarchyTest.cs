using AwesomeAssertions;
using Sagittaras.Scripting.Building;
using Sagittaras.Tests.Scripting.Fixtures;

namespace Sagittaras.Tests.Scripting.Building;

public class ScriptHierarchyTest
{
    private readonly ScriptHierarchy _hierarchy = new();

    /// <summary>
    ///     Verify that a script is registered under every entrypoint above it, the most derived one first.
    /// </summary>
    [Fact]
    public void GetEntrypoints_CollectsWholeChain()
    {
        _hierarchy.GetEntrypoints(typeof(BossScript))
            .Should()
            .Equal(typeof(CreatureScript), typeof(WorldObjectScript), typeof(ObjectScript));
    }

    /// <summary>
    ///     Verify that a level of the hierarchy which did not opt in is not an entrypoint.
    /// </summary>
    /// <remarks>
    ///     <see cref="UnmarkedScript"/> sits between the creature and the world object script, and the
    ///     attribute is not inherited, so it must be skipped while the levels around it are kept.
    /// </remarks>
    [Fact]
    public void GetEntrypoints_SkipsUnmarkedLevel()
    {
        _hierarchy.GetEntrypoints(typeof(BossScript)).Should().NotContain(typeof(UnmarkedScript));
    }

    /// <summary>
    ///     Verify that a script reaching only the middle of the chain is not registered under the levels below it.
    /// </summary>
    [Fact]
    public void GetEntrypoints_StopsAtTheDeclaredLevel()
    {
        _hierarchy.GetEntrypoints(typeof(CrateScript))
            .Should()
            .Equal(typeof(WorldObjectScript), typeof(ObjectScript));
    }

    /// <summary>
    ///     Verify that an entrypoint asked about itself is included in its own chain.
    /// </summary>
    [Fact]
    public void GetEntrypoints_IncludesTheEntrypointItself()
    {
        _hierarchy.GetEntrypoints(typeof(CreatureScript)).Should().Contain(typeof(CreatureScript));
    }

    /// <summary>
    ///     Verify that a script deriving from no entrypoint resolves to nothing.
    /// </summary>
    [Fact]
    public void GetEntrypoints_EmptyForScriptWithoutEntrypoint()
    {
        _hierarchy.GetEntrypoints(typeof(OrphanScript)).Should().BeEmpty();
    }

    /// <summary>
    ///     Verify that the resolved chain is cached instead of walked again.
    /// </summary>
    [Fact]
    public void GetEntrypoints_CachesResolvedChain()
    {
        _hierarchy.GetEntrypoints(typeof(BossScript))
            .Should()
            .BeSameAs(_hierarchy.GetEntrypoints(typeof(BossScript)));
    }
}
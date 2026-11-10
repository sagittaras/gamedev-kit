using System.Reflection;
using AwesomeAssertions;
using Sagittaras.Scripting.Discovery;
using Sagittaras.Tests.Scripting.Module;

namespace Sagittaras.Tests.Scripting.Discovery;

public class ScriptDiscoveryTest
{
    private static readonly Assembly ModuleAssembly = typeof(GoblinScript).Assembly;

    private readonly ScriptDiscovery _discovery = new();

    /// <summary>
    ///     Verify that scanning finds every script of the module and nothing else.
    /// </summary>
    [Fact]
    public void Scan_FindsEveryScriptOfTheModule()
    {
        _discovery.Scan(ModuleAssembly)
            .Select(descriptor => descriptor.ScriptType)
            .Should()
            .BeEquivalentTo([
                typeof(GoblinScript),
                typeof(GoblinBossScript),
                typeof(BarrelScript),
                typeof(ModuleRulesScript),
                typeof(BeastScript),
                typeof(SmallContainerScript)
            ]);
    }

    /// <summary>
    ///     Verify that each script is sorted into the registry it belongs to.
    /// </summary>
    [Fact]
    public void Scan_SortsScriptsByTheirKind()
    {
        Dictionary<Type, ScriptKind> kinds = _discovery.Scan(ModuleAssembly)
            .ToDictionary(descriptor => descriptor.ScriptType, descriptor => descriptor.Kind);

        kinds[typeof(GoblinScript)].Should().Be(ScriptKind.Transient);
        kinds[typeof(GoblinBossScript)].Should().Be(ScriptKind.Transient);
        kinds[typeof(BarrelScript)].Should().Be(ScriptKind.Entity);
        kinds[typeof(ModuleRulesScript)].Should().Be(ScriptKind.Global);
    }

    /// <summary>
    ///     Verify that the entity a script names is carried over from its attribute.
    /// </summary>
    [Fact]
    public void Scan_ReadsTheNamedEntity()
    {
        _discovery.Scan(ModuleAssembly)
            .Single(descriptor => descriptor.ScriptType == typeof(GoblinScript))
            .EntityIds
            .Should()
            .Equal("goblin");
    }

    /// <summary>
    ///     Verify that the abstract entrypoints themselves are not registered as scripts.
    /// </summary>
    [Fact]
    public void Scan_SkipsEntrypoints()
    {
        _discovery.Scan(ModuleAssembly)
            .Select(descriptor => descriptor.ScriptType)
            .Should()
            .NotContain([typeof(UnitScript), typeof(EliteUnitScript), typeof(ContainerScript), typeof(RulesScript)]);
    }

    /// <summary>
    ///     Verify that a script deriving from no entrypoint is walked past instead of taking the scan down.
    /// </summary>
    [Fact]
    public void Scan_SkipsScriptWithoutEntrypoint()
    {
        _discovery.Scan(ModuleAssembly)
            .Select(descriptor => descriptor.ScriptType)
            .Should()
            .NotContain(typeof(LooseScript));
    }

    /// <summary>
    ///     Verify that a script naming several entities carries all of them.
    /// </summary>
    [Fact]
    public void Scan_ReadsEveryNamedEntity()
    {
        _discovery.Scan(ModuleAssembly)
            .Single(descriptor => descriptor.ScriptType == typeof(BeastScript))
            .EntityIds
            .Should()
            .Equal("wolf", "dire_wolf");
    }
}
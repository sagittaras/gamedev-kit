using AwesomeAssertions;
using Sagittaras.Scripting.Building;
using Sagittaras.Scripting;
using Sagittaras.Scripting.Attributes;
using Sagittaras.Scripting.Discovery;
using Sagittaras.Scripting.Entities;
using Sagittaras.Tests.Scripting.Fixtures;

namespace Sagittaras.Tests.Scripting.Discovery;

public class ScriptDiscoveryFailureTest
{
    /// <summary>
    ///     Verify that a script serving an entity but naming none is reported.
    /// </summary>
    /// <remarks>
    ///     Nothing could ever reach it, and silently dropping it would leave the entity behaving as if the
    ///     script was never written.
    /// </remarks>
    [Fact]
    public void Describe_ReportsEntityScriptWithoutNamedEntity()
    {
        FluentActions.Invoking(() => ScriptDiscovery.Describe(typeof(NamelessLootScript)))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage($"*{nameof(ScriptedEntityAttribute)}*");
    }

    /// <summary>
    ///     Verify that a global script naming an entity it cannot serve is reported.
    /// </summary>
    [Fact]
    public void Describe_ReportsGlobalScriptNamingAnEntity()
    {
        FluentActions.Invoking(() => ScriptDiscovery.Describe(typeof(NamedRulesScript)))
            .Should()
            .Throw<ArgumentException>();
    }

    /// <summary>
    ///     Verify that a script belonging to two registries at once is reported instead of guessed.
    /// </summary>
    [Fact]
    public void Describe_ReportsScriptOfSeveralKinds()
    {
        FluentActions.Invoking(() => ScriptDiscovery.Describe(typeof(ConfusedScript)))
            .Should()
            .Throw<ArgumentException>();
    }

    /// <summary>
    ///     Verify that a transient script the activator could not create is reported with the reason.
    /// </summary>
    /// <remarks>
    ///     Its state belongs to the initialization, so a constructor taking arguments is a mistake the
    ///     message has to name - the constraint of the builder cannot be checked at compile time here.
    /// </remarks>
    [Fact]
    public void Register_ReportsTransientScriptWithoutParameterlessConstructor()
    {
        ScriptDiscovery discovery = new();
        ScriptDescriptor descriptor = ScriptDescriptor.Transient(typeof(ConstructedScript), ["beast"]);

        discovery.Invoking(d => d.Register(new ScriptRegistryBuilder(), descriptor))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("*parameterless constructor*");
    }
}

/// <summary>
///     Serves an entity, but names none.
/// </summary>
public class NamelessLootScript : LootableScript;

/// <summary>
///     Extends a hook-point without an entity target, yet names an entity.
/// </summary>
[ScriptedEntity("chest")]
[ScriptedEntity("crate")]
public class NamedRulesScript : DifficultyScript;

/// <summary>
///     Belongs to the shared and the global registry at once.
/// </summary>
public class ConfusedScript : LootableScript, IGlobalScript;

/// <summary>
///     Transient script taking its state through the constructor instead of the initialization.
/// </summary>
[ScriptedEntity("beast")]
public class ConstructedScript(int rage) : CreatureScript
{
    public int Rage { get; } = rage;
}
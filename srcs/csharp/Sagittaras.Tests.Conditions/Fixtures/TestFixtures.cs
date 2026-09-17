using Sagittaras.Conditions;
using Sagittaras.Conditions.Handlers;
using Sagittaras.Conditions.Targeting;

namespace Sagittaras.Tests.Conditions.Fixtures;

/// <summary>
///     Condition types and targets used across the tests.
/// </summary>
public static class TestKeys
{
    public static readonly ConditionType Level = new("test.level");
    public static readonly ConditionType Alive = new("test.alive");

    public static readonly ConditionTarget Self = new("test.self");
    public static readonly ConditionTarget Target = new("test.target");
}

/// <summary>
///     Game unit used as the subject of evaluation.
/// </summary>
public record TestUnit(int Level);

/// <summary>
///     Evaluation context with a mandatory caster and an optional target.
/// </summary>
public record CastContext(TestUnit Caster, TestUnit? Target = null);

/// <summary>
///     Plain data implementation of the condition.
/// </summary>
public record TestCondition(
    ConditionType ConditionType,
    ConditionTarget Target,
    IReadOnlyList<int> Parameters,
    Comparison Comparison = Comparison.Equals,
    bool Negated = false) : ICondition;

/// <summary>
///     Plain data implementation of the conditional.
/// </summary>
public record TestConditional(IReadOnlyList<ICondition> Conditions) : IConditional;

/// <summary>
///     Compares the level of the unit.
/// </summary>
public class LevelHandler : ComparisonHandler<TestUnit>
{
    protected override int GetActualValue(ICondition condition, TestUnit subject)
    {
        return subject.Level;
    }
}

/// <summary>
///     Resolves the caster, which is always present in the context.
/// </summary>
public class CasterResolver : ITargetResolver<CastContext, TestUnit>
{
    public bool TryResolve(CastContext context, out TestUnit subject)
    {
        subject = context.Caster;
        return true;
    }
}

/// <summary>
///     Handler counting its invocations, used to verify evaluation short-circuiting.
/// </summary>
public class CountingHandler(bool result) : IConditionHandler<TestUnit>
{
    public int Invocations { get; private set; }

    public bool Evaluate(ICondition condition, TestUnit subject)
    {
        Invocations++;
        return result;
    }
}

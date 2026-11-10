using AwesomeAssertions;
using Sagittaras.Conditions;
using Sagittaras.Conditions.Evaluation;
using Sagittaras.Tests.Conditions.Fixtures;

namespace Sagittaras.Tests.Conditions.Evaluation;

public class ConditionManagerTest
{
    private readonly ConditionEvaluator<CastContext, TestUnit> _castEvaluator = new ConditionEvaluatorBuilder<CastContext, TestUnit>()
        .WithHandler(TestKeys.Level, new LevelHandler())
        .WithResolver(TestKeys.Self, new CasterResolver())
        .Build();

    private readonly ConditionEvaluator<QuestContext, TestUnit> _questEvaluator = new ConditionEvaluatorBuilder<QuestContext, TestUnit>()
        .WithHandler(TestKeys.Level, new LevelHandler())
        .WithResolver(TestKeys.Self, new PlayerResolver())
        .Build();

    private readonly TestCondition _condition = new(TestKeys.Level, TestKeys.Self, [5]);

    /// <summary>
    ///     Creates the manager with evaluators of both test contexts.
    /// </summary>
    private ConditionManager CreateManager()
    {
        return new ConditionManagerBuilder()
            .WithEvaluator(_castEvaluator)
            .WithEvaluator(_questEvaluator)
            .Build();
    }

    /// <summary>
    ///     Verify that a condition is evaluated by the evaluator registered for the used context.
    /// </summary>
    [Fact]
    public void Evaluate_ConditionDispatchedByContext()
    {
        IConditionManager manager = CreateManager();

        manager.Evaluate(_condition, new CastContext(new TestUnit(5))).Should().BeTrue();
        manager.Evaluate(_condition, new CastContext(new TestUnit(1))).Should().BeFalse();
        manager.Evaluate(_condition, new QuestContext(new TestUnit(5))).Should().BeTrue();
        manager.Evaluate(_condition, new QuestContext(new TestUnit(1))).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that a conditional is evaluated by the evaluator registered for the used context.
    /// </summary>
    [Fact]
    public void Evaluate_ConditionalDispatchedByContext()
    {
        IConditionManager manager = CreateManager();
        TestConditional conditional = new([
            _condition,
            new TestCondition(TestKeys.Level, TestKeys.Self, [3], Comparison.GreaterOrEqual)
        ]);

        manager.Evaluate(conditional, new CastContext(new TestUnit(5))).Should().BeTrue();
        manager.Evaluate(conditional, new QuestContext(new TestUnit(3))).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that an unregistered context type throws with both the used and the registered types.
    /// </summary>
    [Fact]
    public void Evaluate_EvaluatorNotRegistered()
    {
        IConditionManager manager = new ConditionManagerBuilder()
            .WithEvaluator(_castEvaluator)
            .Build();

        manager.Invoking(x => x.Evaluate(_condition, new QuestContext(new TestUnit(5))))
            .Should().Throw<ConditionEvaluatorNotRegisteredException>()
            .Where(x => x.ContextType == typeof(QuestContext))
            .WithMessage($"*{typeof(QuestContext)}*{typeof(CastContext)}*");
    }

    /// <summary>
    ///     Verify the message of the exception when the manager has no evaluator at all.
    /// </summary>
    [Fact]
    public void Evaluate_NoEvaluatorRegistered()
    {
        IConditionManager manager = new ConditionManagerBuilder().Build();

        manager.Invoking(x => x.Evaluate(_condition, new CastContext(new TestUnit(5))))
            .Should().Throw<ConditionEvaluatorNotRegisteredException>()
            .WithMessage("*No context types are registered.*");
    }

    /// <summary>
    ///     Verify that the registered evaluator can be taken out of the manager.
    /// </summary>
    [Fact]
    public void TryGetEvaluator_Registered()
    {
        IConditionManager manager = CreateManager();

        manager.TryGetEvaluator(out IConditionEvaluator<CastContext>? evaluator).Should().BeTrue();
        evaluator.Should().BeSameAs(_castEvaluator);
    }

    /// <summary>
    ///     Verify that an unregistered context type returns no evaluator instead of throwing.
    /// </summary>
    [Fact]
    public void TryGetEvaluator_NotRegistered()
    {
        IConditionManager manager = new ConditionManagerBuilder()
            .WithEvaluator(_castEvaluator)
            .Build();

        manager.TryGetEvaluator(out IConditionEvaluator<QuestContext>? evaluator).Should().BeFalse();
        evaluator.Should().BeNull();
    }
}

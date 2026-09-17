using AwesomeAssertions;
using Sagittaras.Conditions;
using Sagittaras.Conditions.Evaluation;
using Sagittaras.Tests.Conditions.Fixtures;

namespace Sagittaras.Tests.Conditions.Evaluation;

public class ConditionEvaluatorTest
{
    private readonly ConditionEvaluator<CastContext, TestUnit> _evaluator = new ConditionEvaluatorBuilder<CastContext, TestUnit>()
        .WithHandler(TestKeys.Level, new LevelHandler())
        .WithResolver(TestKeys.Self, new CasterResolver())
        .WithResolver(TestKeys.Target, context => context.Target)
        .Build();

    /// <summary>
    ///     Verify evaluation of a satisfied and an unsatisfied condition.
    /// </summary>
    [Theory]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public void Evaluate_Condition(int expectedLevel, bool result)
    {
        TestCondition condition = new(TestKeys.Level, TestKeys.Self, [expectedLevel]);

        _evaluator.Evaluate(condition, new CastContext(new TestUnit(5))).Should().Be(result);
    }

    /// <summary>
    ///     Verify that negation inverts the result of the handler.
    /// </summary>
    [Theory]
    [InlineData(5, false)]
    [InlineData(6, true)]
    public void Evaluate_NegatedCondition(int expectedLevel, bool result)
    {
        TestCondition condition = new(TestKeys.Level, TestKeys.Self, [expectedLevel], Negated: true);

        _evaluator.Evaluate(condition, new CastContext(new TestUnit(5))).Should().Be(result);
    }

    /// <summary>
    ///     Verify that the subject is resolved by the resolver registered for the condition target.
    /// </summary>
    [Fact]
    public void Evaluate_ResolvesSubjectByTarget()
    {
        CastContext context = new(new TestUnit(1), new TestUnit(9));
        TestCondition condition = new(TestKeys.Level, TestKeys.Target, [9]);

        _evaluator.Evaluate(condition, context).Should().BeTrue();
    }

    /// <summary>
    ///     Verify that an unresolved subject fails the condition regardless of negation.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Evaluate_UnresolvedSubject(bool negated)
    {
        TestCondition condition = new(TestKeys.Level, TestKeys.Target, [5], Negated: negated);

        _evaluator.Evaluate(condition, new CastContext(new TestUnit(5))).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that a condition type without a registered handler throws.
    /// </summary>
    [Fact]
    public void Evaluate_HandlerNotRegistered()
    {
        TestCondition condition = new(TestKeys.Alive, TestKeys.Self, []);

        _evaluator.Invoking(x => x.Evaluate(condition, new CastContext(new TestUnit(5))))
            .Should().Throw<ConditionHandlerNotRegisteredException>()
            .Which.ConditionType.Should().Be(TestKeys.Alive);
    }

    /// <summary>
    ///     Verify that a condition target without a registered resolver throws.
    /// </summary>
    [Fact]
    public void Evaluate_ResolverNotRegistered()
    {
        ConditionTarget party = new("test.party");
        TestCondition condition = new(TestKeys.Level, party, [5]);

        _evaluator.Invoking(x => x.Evaluate(condition, new CastContext(new TestUnit(5))))
            .Should().Throw<TargetResolverNotRegisteredException>()
            .Which.Target.Should().Be(party);
    }

    /// <summary>
    ///     Verify that a conditional is satisfied when all its conditions are satisfied.
    /// </summary>
    [Fact]
    public void Evaluate_ConditionalAllSatisfied()
    {
        TestConditional conditional = new([
            new TestCondition(TestKeys.Level, TestKeys.Self, [3], Comparison.GreaterOrEqual),
            new TestCondition(TestKeys.Level, TestKeys.Self, [9], Comparison.LessThan)
        ]);

        _evaluator.Evaluate(conditional, new CastContext(new TestUnit(5))).Should().BeTrue();
    }

    /// <summary>
    ///     Verify that a conditional without conditions is satisfied.
    /// </summary>
    [Fact]
    public void Evaluate_ConditionalEmpty()
    {
        _evaluator.Evaluate(new TestConditional([]), new CastContext(new TestUnit(5))).Should().BeTrue();
    }

    /// <summary>
    ///     Verify that evaluation of a conditional stops at the first unsatisfied condition.
    /// </summary>
    [Fact]
    public void Evaluate_ConditionalStopsAtFirstFailure()
    {
        ConditionType failing = new("test.failing");
        ConditionType counted = new("test.counted");
        CountingHandler failingHandler = new(false);
        CountingHandler countedHandler = new(true);

        ConditionEvaluator<CastContext, TestUnit> evaluator = new ConditionEvaluatorBuilder<CastContext, TestUnit>()
            .WithHandler(failing, failingHandler)
            .WithHandler(counted, countedHandler)
            .WithResolver(TestKeys.Self, new CasterResolver())
            .Build();

        TestConditional conditional = new([
            new TestCondition(failing, TestKeys.Self, []),
            new TestCondition(counted, TestKeys.Self, [])
        ]);

        evaluator.Evaluate(conditional, new CastContext(new TestUnit(5))).Should().BeFalse();
        failingHandler.Invocations.Should().Be(1);
        countedHandler.Invocations.Should().Be(0);
    }
}

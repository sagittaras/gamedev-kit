using AwesomeAssertions;
using Sagittaras.Conditions;
using Sagittaras.Conditions.Evaluation;
using Sagittaras.Tests.Conditions.Fixtures;

namespace Sagittaras.Tests.Conditions.Evaluation;

public class ConditionEvaluatorBuilderTest
{
    private readonly ConditionEvaluatorBuilder<CastContext, TestUnit> _builder = new();

    /// <summary>
    ///     Verify that the same condition type cannot be registered twice.
    /// </summary>
    [Fact]
    public void WithHandler_Duplicate()
    {
        _builder.WithHandler(TestKeys.Level, new LevelHandler());

        _builder.Invoking(x => x.WithHandler(TestKeys.Level, (_, _) => true))
            .Should().Throw<ArgumentException>()
            .WithMessage($"*{TestKeys.Level}*");
    }

    /// <summary>
    ///     Verify that the same condition target cannot be registered twice.
    /// </summary>
    [Fact]
    public void WithResolver_Duplicate()
    {
        _builder.WithResolver(TestKeys.Self, new CasterResolver());

        _builder.Invoking(x => x.WithResolver(TestKeys.Self, context => context.Caster))
            .Should().Throw<ArgumentException>()
            .WithMessage($"*{TestKeys.Self}*");
    }

    /// <summary>
    ///     Verify that a handler given as a function is used for evaluation.
    /// </summary>
    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void WithHandler_Function(int level, bool result)
    {
        ConditionEvaluator<CastContext, TestUnit> evaluator = _builder
            .WithHandler(TestKeys.Alive, (_, unit) => unit.Level > 0)
            .WithResolver(TestKeys.Self, new CasterResolver())
            .Build();

        TestCondition condition = new(TestKeys.Alive, TestKeys.Self, []);
        evaluator.Evaluate(condition, new CastContext(new TestUnit(level))).Should().Be(result);
    }

    /// <summary>
    ///     Verify that a resolver given as a function resolves the subject, and returning null leaves it unresolved.
    /// </summary>
    [Fact]
    public void WithResolver_Function()
    {
        ConditionEvaluator<CastContext, TestUnit> evaluator = _builder
            .WithHandler(TestKeys.Alive, (_, _) => true)
            .WithResolver(TestKeys.Target, context => context.Target)
            .Build();

        TestCondition condition = new(TestKeys.Alive, TestKeys.Target, [], Negated: true);
        TestCondition notNegated = condition with { Negated = false };

        evaluator.Evaluate(notNegated, new CastContext(new TestUnit(1), new TestUnit(1))).Should().BeTrue();
        evaluator.Evaluate(condition, new CastContext(new TestUnit(1))).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that registrations made after <see cref="ConditionEvaluatorBuilder{TContext,TSubject}.Build"/>
    ///     do not affect the already built evaluator, but are included in a new one.
    /// </summary>
    [Fact]
    public void Build_IsolatedFromBuilder()
    {
        _builder.WithResolver(TestKeys.Self, new CasterResolver());
        ConditionEvaluator<CastContext, TestUnit> first = _builder.Build();

        _builder.WithHandler(TestKeys.Alive, (_, _) => true);
        ConditionEvaluator<CastContext, TestUnit> second = _builder.Build();

        TestCondition condition = new(TestKeys.Alive, TestKeys.Self, []);
        CastContext context = new(new TestUnit(1));

        first.Invoking(x => x.Evaluate(condition, context)).Should().Throw<ConditionHandlerNotRegisteredException>();
        second.Evaluate(condition, context).Should().BeTrue();
    }
}

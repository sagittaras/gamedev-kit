using AwesomeAssertions;
using Sagittaras.Conditions.Evaluation;
using Sagittaras.Tests.Conditions.Fixtures;

namespace Sagittaras.Tests.Conditions.Evaluation;

public class ConditionManagerBuilderTest
{
    private readonly ConditionManagerBuilder _builder = new();

    private readonly ConditionEvaluator<CastContext, TestUnit> _evaluator = new ConditionEvaluatorBuilder<CastContext, TestUnit>()
        .WithHandler(TestKeys.Level, new LevelHandler())
        .WithResolver(TestKeys.Self, new CasterResolver())
        .Build();

    /// <summary>
    ///     Verify that the same context type cannot be registered twice.
    /// </summary>
    [Fact]
    public void WithEvaluator_Duplicate()
    {
        _builder.WithEvaluator(_evaluator);

        _builder.Invoking(x => x.WithEvaluator(_evaluator))
            .Should().Throw<ArgumentException>()
            .WithMessage($"*{typeof(CastContext)}*");
    }

    /// <summary>
    ///     Verify that registrations made after <see cref="ConditionManagerBuilder.Build"/> do not affect
    ///     the already built manager, but are included in a new one.
    /// </summary>
    [Fact]
    public void Build_IsolatedFromBuilder()
    {
        ConditionManager first = _builder.Build();

        _builder.WithEvaluator(_evaluator);
        ConditionManager second = _builder.Build();

        first.TryGetEvaluator(out IConditionEvaluator<CastContext>? _).Should().BeFalse();
        second.TryGetEvaluator(out IConditionEvaluator<CastContext>? _).Should().BeTrue();
    }
}

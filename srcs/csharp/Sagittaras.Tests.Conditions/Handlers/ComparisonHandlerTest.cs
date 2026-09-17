using AwesomeAssertions;
using Sagittaras.Conditions;
using Sagittaras.Conditions.Handlers;
using Sagittaras.Tests.Conditions.Fixtures;

namespace Sagittaras.Tests.Conditions.Handlers;

public class ComparisonHandlerTest
{
    private readonly LevelHandler _handler = new();
    private readonly TestUnit _unit = new(5);

    /// <summary>
    ///     Verify each comparison operator against the subject's value of 5.
    /// </summary>
    [Theory]
    [InlineData(Comparison.Equals, 5, true)]
    [InlineData(Comparison.Equals, 4, false)]
    [InlineData(Comparison.GreaterThan, 4, true)]
    [InlineData(Comparison.GreaterThan, 5, false)]
    [InlineData(Comparison.LessThan, 6, true)]
    [InlineData(Comparison.LessThan, 5, false)]
    [InlineData(Comparison.GreaterOrEqual, 5, true)]
    [InlineData(Comparison.GreaterOrEqual, 6, false)]
    [InlineData(Comparison.LessOrEqual, 5, true)]
    [InlineData(Comparison.LessOrEqual, 4, false)]
    public void Evaluate_Comparison(Comparison comparison, int expected, bool result)
    {
        TestCondition condition = new(TestKeys.Level, TestKeys.Self, [expected], comparison);

        _handler.Evaluate(condition, _unit).Should().Be(result);
    }

    /// <summary>
    ///     Verify that the expected value can be taken from a different parameter.
    /// </summary>
    [Fact]
    public void Evaluate_OverriddenExpectedValue()
    {
        ItemAmountHandler handler = new();
        TestCondition condition = new(TestKeys.Level, TestKeys.Self, [ItemAmountHandler.ItemId, 3], Comparison.GreaterOrEqual);

        handler.Evaluate(condition, _unit).Should().BeTrue();
    }

    /// <summary>
    ///     Verify that a condition without parameters is rejected with an explicit exception.
    /// </summary>
    [Fact]
    public void Evaluate_MissingParameter()
    {
        TestCondition condition = new(TestKeys.Level, TestKeys.Self, []);

        _handler.Invoking(x => x.Evaluate(condition, _unit))
            .Should().Throw<ArgumentException>()
            .WithMessage($"*{TestKeys.Level}*");
    }

    /// <summary>
    ///     Verify that an undefined comparison operator is rejected.
    /// </summary>
    [Fact]
    public void Evaluate_UnknownComparison()
    {
        TestCondition condition = new(TestKeys.Level, TestKeys.Self, [5], (Comparison)99);

        _handler.Invoking(x => x.Evaluate(condition, _unit))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage($"*{TestKeys.Level}*");
    }

    /// <summary>
    ///     Compares the amount of an item identified by the first parameter against the second parameter.
    /// </summary>
    private class ItemAmountHandler : ComparisonHandler<TestUnit>
    {
        public const int ItemId = 42;

        protected override int GetActualValue(ICondition condition, TestUnit subject)
        {
            return condition.Parameters[0] == ItemId ? 3 : 0;
        }

        protected override int GetExpectedValue(ICondition condition)
        {
            return condition.Parameters[1];
        }
    }
}

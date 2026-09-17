using AwesomeAssertions;
using Sagittaras.Conditions;

namespace Sagittaras.Tests.Conditions;

public class ConditionTargetTest
{
    /// <summary>
    ///     Verify that condition targets with the same identifier are equal.
    /// </summary>
    [Fact]
    public void Equals_SameIdentifier()
    {
        ConditionTarget left = new("self");
        ConditionTarget right = new("self");

        left.Equals(right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    /// <summary>
    ///     Verify that condition targets with different identifiers are not equal.
    /// </summary>
    [Fact]
    public void Equals_DifferentIdentifier()
    {
        ConditionTarget left = new("self");
        ConditionTarget right = new("target");

        left.Equals(right).Should().BeFalse();
        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    /// <summary>
    ///     Verify that the default value is usable without throwing.
    /// </summary>
    [Fact]
    public void Default_IsUsable()
    {
        ConditionTarget value = default;

        value.GetHashCode().Should().Be(0);
        value.ToString().Should().BeEmpty();
        value.Should().Be(default(ConditionTarget));
        value.Should().NotBe(new ConditionTarget("self"));
    }

    /// <summary>
    ///     Verify that <see cref="ConditionTarget.ToString"/> returns the identifier.
    /// </summary>
    [Fact]
    public void ToString_ReturnsIdentifier()
    {
        new ConditionTarget("self").ToString().Should().Be("self");
    }
}

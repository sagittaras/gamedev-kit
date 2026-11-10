using AwesomeAssertions;
using Sagittaras.Conditions;

namespace Sagittaras.Tests.Conditions;

public class ConditionTypeTest
{
    /// <summary>
    ///     Verify that condition types with the same identifier are equal.
    /// </summary>
    [Fact]
    public void Equals_SameIdentifier()
    {
        ConditionType left = new("level");
        ConditionType right = new("level");

        left.Equals(right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    /// <summary>
    ///     Verify that condition types with different identifiers are not equal.
    /// </summary>
    [Fact]
    public void Equals_DifferentIdentifier()
    {
        ConditionType left = new("level");
        ConditionType right = new("aura");

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
        ConditionType value = default;

        value.GetHashCode().Should().Be(0);
        value.ToString().Should().BeEmpty();
        value.Should().Be(default(ConditionType));
        value.Should().NotBe(new ConditionType("level"));
    }

    /// <summary>
    ///     Verify that <see cref="ConditionType.ToString"/> returns the identifier.
    /// </summary>
    [Fact]
    public void ToString_ReturnsIdentifier()
    {
        new ConditionType("level").ToString().Should().Be("level");
    }
}

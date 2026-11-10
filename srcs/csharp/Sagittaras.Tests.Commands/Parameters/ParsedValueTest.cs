using AwesomeAssertions;
using Sagittaras.Commands.Parameters;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands.Parameters;

public class ParsedValueTest
{
    /// <summary>
    ///     Verify that a value that was read carries it, and no reason.
    /// </summary>
    [Fact]
    public void Read_CarriesTheValue()
    {
        ParsedValue<int> parsed = ParsedValue<int>.Read(40);

        parsed.IsRead.Should().BeTrue();
        parsed.Value.Should().Be(40);
        parsed.Message.Should().BeNull();
    }

    /// <summary>
    ///     Verify that a refusal carries its reason, and no value.
    /// </summary>
    [Fact]
    public void Refused_CarriesTheReason()
    {
        ParsedValue<int> parsed = ParsedValue<int>.Refused("Too much.");

        parsed.IsRead.Should().BeFalse();
        parsed.Message.Should().Be("Too much.");
        parsed.Value.Should().Be(0);
    }

    /// <summary>
    ///     Verify that a refusal has to say why.
    /// </summary>
    /// <remarks>
    ///     The reason is what reaches the console, so an empty one is a mistake in the parser worth hearing
    ///     about where it is written.
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Refused_RequiresAReason(string message)
    {
        Action refusing = () => ParsedValue<int>.Refused(message);

        refusing.Should().Throw<ArgumentException>();
    }

    /// <summary>
    ///     Verify that the default of the struct is a refusal with no reason.
    /// </summary>
    [Fact]
    public void Default_IsAReasonlessRefusal()
    {
        ParsedValue<int> parsed = default;

        parsed.IsRead.Should().BeFalse();
        parsed.Message.Should().BeNull();
    }

    /// <summary>
    ///     Verify that a parameter without a parser of its own leaves the reading to the converter.
    /// </summary>
    [Fact]
    public void Parser_IsAbsentUnlessGiven()
    {
        IParameter parameter = SpawnEntityCommand.Count;

        parameter.Parser.Should().BeNull();
    }

    /// <summary>
    ///     Verify that the binding reaches the typed parser through the untyped contract, the value intact.
    /// </summary>
    [Fact]
    public void Parser_ReachesTheTypedParserUntyped()
    {
        IParameter parameter = HealCommand.Percent;

        ParsedValue<object?> parsed = parameter.Parser!("half");

        parsed.IsRead.Should().BeTrue();
        parsed.Value.Should().Be(50);
    }

    /// <summary>
    ///     Verify that a refusal survives the way to the untyped contract with its reason.
    /// </summary>
    [Fact]
    public void Parser_KeepsTheReasonUntyped()
    {
        IParameter parameter = HealCommand.Percent;

        ParsedValue<object?> parsed = parameter.Parser!("150");

        parsed.IsRead.Should().BeFalse();
        parsed.Message.Should().Contain("between 0 and 100");
    }
}

using AwesomeAssertions;
using Sagittaras.Commands;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parameters;
using Sagittaras.Commands.Parsing;
using Sagittaras.Commands.Parsing.Errors;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands.Parsing;

public class ValidatorTest
{
    private static readonly IReadOnlyList<ICommand> Commands = [new SummonCommand(), new StrangeCommand()];

    private readonly CommandParser _parser = new();

    /// <summary>
    ///     Verify that a value the validator accepts reaches the command.
    /// </summary>
    [Fact]
    public void Parse_TakesAnAcceptedValue()
    {
        _parser.Parse("summon 5", Commands).Input!.GetValue<int>(SummonCommand.Count).Should().Be(5);
    }

    /// <summary>
    ///     Verify that a value the validator refuses is reported in its words, pointing at the value.
    /// </summary>
    [Fact]
    public void Parse_ReportsARefusalOfTheValidator()
    {
        IParseError error = _parser.Parse("summon 50", Commands).Error!;

        error.Code.Should().Be(ParseErrorCode.ValueRefused);
        error.Message.Should().Be("Between 1 and 10 can be summoned at once, not 50.");
        error.Token!.Value.Lexeme.Should().Be("50");
    }

    /// <summary>
    ///     Verify that the validator leaves the reading to the converter, and is not reached without a value.
    /// </summary>
    /// <remarks>
    ///     Which is the whole point of a validator next to a parser: the number is read the way every other
    ///     number of the input is, and only then judged.
    /// </remarks>
    [Fact]
    public void Parse_LeavesTheReadingToTheConverter()
    {
        _parser.Parse("summon many", Commands).Error!.Code.Should().Be(ParseErrorCode.ValueTypeMismatch);
    }

    /// <summary>
    ///     Verify that the validator judges what the parser made of the text, not the text itself.
    /// </summary>
    [Fact]
    public void Parse_JudgesWhatTheParserRead()
    {
        IParseError error = _parser.Parse("summon 2 --creature drake", Commands).Error!;

        error.Code.Should().Be(ParseErrorCode.ValueRefused);
        error.Message.Should().Be("Dragons cannot be summoned.");
    }

    /// <summary>
    ///     Verify that a value both the parser and the validator accept reaches the command.
    /// </summary>
    [Fact]
    public void Parse_TakesWhatBothAccept()
    {
        _parser.Parse("summon 2 -c goblin", Commands).Input!.GetValue<string>(SummonCommand.Creature).Should().Be("goblin");
    }

    /// <summary>
    ///     Verify that a default is not judged, as it never came from the input.
    /// </summary>
    [Fact]
    public void Parse_LeavesTheDefaultUnjudged()
    {
        IParseResult result = _parser.Parse("strange", Commands);

        result.Success.Should().BeTrue();
        result.Input!.GetValue<int>(StrangeCommand.Level).Should().Be(0);
    }

    /// <summary>
    ///     Verify that a refusal given in blank words is still answered, and plainly.
    /// </summary>
    [Fact]
    public void Parse_AnswersABlankRefusalPlainly()
    {
        IParseError error = _parser.Parse("strange 1 --mood grumpy", Commands).Error!;

        error.Code.Should().Be(ParseErrorCode.ValueRefused);
        error.Message.Should().Be("Option 'mood' does not accept 'grumpy'.");
    }

    /// <summary>
    ///     Verify that a parameter without a validator accepts every value of its type.
    /// </summary>
    [Fact]
    public void Validator_IsAbsentUnlessGiven()
    {
        IParameter parameter = SpawnEntityCommand.Count;

        parameter.Validator.Should().BeNull();
    }

    /// <summary>
    ///     Verify that the binding reaches the typed validator through the untyped contract.
    /// </summary>
    [Fact]
    public void Validator_ReachesTheTypedValidatorUntyped()
    {
        IParameter parameter = SummonCommand.Count;

        parameter.Validator!(5).Should().BeNull();
        parameter.Validator!(50).Should().Contain("not 50");
    }

    /// <summary>
    ///     Command with a default its own validator would refuse, and a validator refusing in blank words.
    /// </summary>
    private class StrangeCommand : Command
    {
        public static readonly Argument<int> Level = new("level", "Level to be strange at", 0)
        {
            Validator = level => level > 0 ? null : "The level has to be above zero.",
        };

        private static readonly Option<string> Mood = new("mood", "Mood to be strange in", "calm")
        {
            Validator = mood => mood == "grumpy" ? "  " : null,
        };

        public StrangeCommand()
            : base("strange", "Behaves strangely")
        {
            Accepts(Level, Mood);
        }

        public override ICommandOutput Invoke(ICommandInput input)
        {
            return CommandOutput.Success();
        }
    }
}

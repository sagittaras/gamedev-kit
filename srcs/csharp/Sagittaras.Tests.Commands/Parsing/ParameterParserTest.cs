using AwesomeAssertions;
using Sagittaras.Commands;
using Sagittaras.Commands.Building;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parameters;
using Sagittaras.Commands.Parsing;
using Sagittaras.Commands.Parsing.Errors;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands.Parsing;

public class ParameterParserTest
{
    private static readonly IReadOnlyList<ICommand> Commands = [new HealCommand(), new ShrugCommand()];

    private readonly CommandParser _parser = new();

    /// <summary>
    ///     Verify that a parameter with a parser of its own is read through it.
    /// </summary>
    [Fact]
    public void Parse_ReadsThroughTheOwnParser()
    {
        _parser.Parse("heal 40", Commands).Input!.GetValue<int>(HealCommand.Percent).Should().Be(40);
    }

    /// <summary>
    ///     Verify that the own parser replaces the converter rather than coming after it.
    /// </summary>
    /// <remarks>
    ///     <c>half</c> is no integer the converter would read, so reaching the command at all proves the
    ///     converter was never asked.
    /// </remarks>
    [Fact]
    public void Parse_LetsTheParserReadWhatTheConverterWouldNot()
    {
        _parser.Parse("heal half", Commands).Input!.GetValue<int>(HealCommand.Percent).Should().Be(50);
    }

    /// <summary>
    ///     Verify that a refusal is reported in the words of the parser, pointing at the value.
    /// </summary>
    [Fact]
    public void Parse_ReportsARefusalInTheParsersOwnWords()
    {
        IParseError error = _parser.Parse("heal 150", Commands).Error!;

        error.Message.Should().Be("The share has to be between 0 and 100, and '150' is not.");
        error.Token!.Value.Lexeme.Should().Be("150");
    }

    /// <summary>
    ///     Verify that a refusal is told apart from a value of the wrong type.
    /// </summary>
    /// <remarks>
    ///     <c>150</c> is a perfectly good integer; it breaks a rule of the parameter, not its type - and a
    ///     console may want to answer the two differently.
    /// </remarks>
    [Fact]
    public void Parse_TellsARefusalFromATypeMismatch()
    {
        _parser.Parse("heal 150", Commands).Error!.Code.Should().Be(ParseErrorCode.ValueRefused);
    }

    /// <summary>
    ///     Verify that an option is read through its own parser, in either way it can be handed a value.
    /// </summary>
    [Theory]
    [InlineData("heal 40 --target companion")]
    [InlineData("heal 40 -t=companion")]
    public void Parse_ReadsAnOptionThroughItsParser(string input)
    {
        _parser.Parse(input, Commands).Input!.GetValue<string>(HealCommand.Target).Should().Be("companion");
    }

    /// <summary>
    ///     Verify that an option refused by its parser is reported in its words.
    /// </summary>
    [Fact]
    public void Parse_ReportsARefusalOfAnOption()
    {
        IParseError error = _parser.Parse("heal 40 --target dragon", Commands).Error!;

        error.Code.Should().Be(ParseErrorCode.ValueRefused);
        error.Message.Should().Be("There is nobody called 'dragon' to heal.");
    }

    /// <summary>
    ///     Verify that a default is not run through the parser, as it never came from the input.
    /// </summary>
    [Fact]
    public void Parse_LeavesTheDefaultUnparsed()
    {
        ICommandInput input = _parser.Parse("heal 40", Commands).Input!;

        input.GetValue<string>(HealCommand.Target).Should().Be("player");
        input.IsSpecified(HealCommand.Target).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that a refusal given without a reason is still answered, and plainly.
    /// </summary>
    /// <remarks>
    ///     The default of the result is such a refusal, so a parser that forgets to say why must not leave
    ///     the console with nothing to print.
    /// </remarks>
    [Fact]
    public void Parse_AnswersAReasonlessRefusalPlainly()
    {
        IParseError error = _parser.Parse("shrug anything", Commands).Error!;

        error.Code.Should().Be(ParseErrorCode.ValueRefused);
        error.Message.Should().Be("Argument 'reason' does not accept 'anything'.");
    }

    /// <summary>
    ///     Verify that the words of the parser reach the console through the manager unchanged.
    /// </summary>
    [Fact]
    public void Execute_HandsTheRefusalToTheConsole()
    {
        ICommandManager manager = new CommandRegistryBuilder().Add(new HealCommand()).Build();

        ICommandOutput output = manager.Execute("heal 150");

        output.ExitCode.Should().Be(RefusalOutput.UsageExitCode);
        output.Content.Should().Be("The share has to be between 0 and 100, and '150' is not.");
    }

    /// <summary>
    ///     Command whose parser refuses everything without saying why.
    /// </summary>
    private class ShrugCommand : Command
    {
        private static readonly Argument<string> Reason = new("reason", "Anything at all")
        {
            Parser = _ => default,
        };

        public ShrugCommand()
            : base("shrug", "Refuses whatever it is told")
        {
            Accepts(Reason);
        }

        public override ICommandOutput Invoke(ICommandInput input)
        {
            return CommandOutput.Success();
        }
    }
}

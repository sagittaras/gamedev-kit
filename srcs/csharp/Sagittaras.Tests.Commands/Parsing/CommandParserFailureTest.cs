using AwesomeAssertions;
using Sagittaras.Commands;
using Sagittaras.Commands.Parsing;
using Sagittaras.Commands.Parsing.Errors;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands.Parsing;

public class CommandParserFailureTest
{
    private static readonly IReadOnlyList<ICommand> Commands = [new SpawnCommand(), new GodCommand()];

    private readonly CommandParser _parser = new();

    /// <summary>
    ///     Verify that an input holding nothing to run is reported rather than resolved.
    /// </summary>
    [Fact]
    public void Parse_ReportsEmptyInput()
    {
        Fail("   ").Code.Should().Be(ParseErrorCode.EmptyInput);
    }

    /// <summary>
    ///     Verify that a first token naming no command is reported, pointing at that very token.
    /// </summary>
    [Fact]
    public void Parse_ReportsUnknownCommand()
    {
        IParseError error = Fail("teleport 3 4");

        error.Code.Should().Be(ParseErrorCode.UnknownCommand);
        error.Token!.Value.Lexeme.Should().Be("teleport");
    }

    /// <summary>
    ///     Verify that a branch reached with a token naming none of its subcommands is reported as such.
    /// </summary>
    /// <remarks>
    ///     Told apart from an unknown command on purpose - the input did get somewhere, and a console can
    ///     answer with what that branch actually offers.
    /// </remarks>
    [Fact]
    public void Parse_ReportsUnknownSubcommand()
    {
        IParseError error = Fail("spawn vehicle");

        error.Code.Should().Be(ParseErrorCode.UnknownSubcommand);
        error.Token!.Value.Lexeme.Should().Be("vehicle");
    }

    /// <summary>
    ///     Verify that an input ending before a required argument is reported.
    /// </summary>
    [Fact]
    public void Parse_ReportsMissingArgument()
    {
        Fail("spawn entity").Code.Should().Be(ParseErrorCode.MissingArgument);
    }

    /// <summary>
    ///     Verify that an argument nobody could be missing has no token to point at.
    /// </summary>
    /// <remarks>
    ///     The input is at fault by what it does not hold, so there is no word in it to underline.
    /// </remarks>
    [Fact]
    public void Parse_LeavesAMissingArgumentWithoutAToken()
    {
        Fail("spawn entity").Token.Should().BeNull();
    }

    /// <summary>
    ///     Verify that a value the command has no argument left for is reported.
    /// </summary>
    [Fact]
    public void Parse_ReportsTooManyArguments()
    {
        IParseError error = Fail("spawn entity goblin 3 extra");

        error.Code.Should().Be(ParseErrorCode.TooManyArguments);
        error.Token!.Value.Lexeme.Should().Be("extra");
    }

    /// <summary>
    ///     Verify that an option the command does not declare is reported.
    /// </summary>
    [Fact]
    public void Parse_ReportsUnknownOption()
    {
        IParseError error = Fail("spawn entity goblin --colour red");

        error.Code.Should().Be(ParseErrorCode.UnknownOption);
        error.Token!.Value.Lexeme.Should().Be("--colour");
    }

    /// <summary>
    ///     Verify that an option left at the end of the input without its value is reported.
    /// </summary>
    [Fact]
    public void Parse_ReportsMissingOptionValueAtTheEnd()
    {
        Fail("spawn entity goblin --scene").Code.Should().Be(ParseErrorCode.MissingOptionValue);
    }

    /// <summary>
    ///     Verify that an option followed by another option is reported instead of swallowing it as a value.
    /// </summary>
    /// <remarks>
    ///     Taking <c>--dry-run</c> for the value of <c>--scene</c> would parse, and then quietly do the wrong
    ///     thing - which is worse than saying the value never followed.
    /// </remarks>
    [Fact]
    public void Parse_ReportsMissingOptionValueBeforeAnotherOption()
    {
        Fail("spawn entity goblin --scene --dry-run").Code.Should().Be(ParseErrorCode.MissingOptionValue);
    }

    /// <summary>
    ///     Verify that a value which cannot be read as the declared type is reported, pointing at it.
    /// </summary>
    [Fact]
    public void Parse_ReportsMismatchedArgumentValue()
    {
        IParseError error = Fail("spawn entity goblin many");

        error.Code.Should().Be(ParseErrorCode.ValueTypeMismatch);
        error.Token!.Value.Lexeme.Should().Be("many");
    }

    /// <summary>
    ///     Verify that a value naming no member of an enum is reported, and the reading names the members.
    /// </summary>
    [Fact]
    public void Parse_ReportsMismatchedEnumValue()
    {
        IParseError error = Fail("spawn entity goblin --mode furious");

        error.Code.Should().Be(ParseErrorCode.ValueTypeMismatch);
        error.Message.Should().Contain("Hostile");
    }

    /// <summary>
    ///     Verify that a failed parsing still carries the command the input got as far as naming.
    /// </summary>
    /// <remarks>
    ///     What lets a console answer a mistyped value with the usage of the command it was meant for.
    /// </remarks>
    [Fact]
    public void Parse_KeepsTheResolvedCommandOnFailure()
    {
        _parser.Parse("spawn entity goblin many", Commands).Command.Should().BeOfType<SpawnEntityCommand>();
    }

    /// <summary>
    ///     Verify that a failed parsing binds nothing, as there is no input a command could be run over.
    /// </summary>
    [Fact]
    public void Parse_BindsNothingOnFailure()
    {
        _parser.Parse("spawn entity goblin many", Commands).Input.Should().BeNull();
    }

    /// <summary>
    ///     Verify that the token the parsing stumbled on is among the ones left without a role.
    /// </summary>
    [Fact]
    public void Parse_LeavesTheOffendingTokenUnmatched()
    {
        IParseResult result = _parser.Parse("spawn entity goblin many", Commands);

        result.UnmatchedTokens.Select(token => token.Lexeme).Should().Contain("many");
        result.UnmatchedTokens.Should().BeSubsetOf(result.Tokens);
    }

    /// <summary>
    ///     Verify that the parsing stops at the first thing it cannot make sense of.
    /// </summary>
    /// <remarks>
    ///     Reporting the second problem of a line whose first word was already wrong would be guessing, so
    ///     there is only ever one reason to answer with.
    /// </remarks>
    [Fact]
    public void Parse_StopsAtTheFirstProblem()
    {
        Fail("spawn entity goblin many --colour red").Code.Should().Be(ParseErrorCode.ValueTypeMismatch);
    }

    /// <summary>
    ///     Verify that a subcommand named behind an option of its branch is answered by saying so.
    /// </summary>
    /// <remarks>
    ///     The path is spelled out before anything else, so <c>spawn --list entity</c> never reaches the
    ///     leaf. Answering that the branch takes no arguments would be true and useless - what the line got
    ///     wrong is the order.
    /// </remarks>
    [Fact]
    public void Parse_ReportsASubcommandNamedTooLate()
    {
        IParseError error = Fail("spawn --list entity goblin");

        error.Code.Should().Be(ParseErrorCode.UnknownSubcommand);
        error.Message.Should().Contain("before the options");
        error.Token!.Value.Lexeme.Should().Be("entity");
    }

    /// <summary>
    ///     Verify that a stray value behind a branch is still answered as a subcommand that is not there.
    /// </summary>
    [Fact]
    public void Parse_ReportsAStrayValueBehindABranch()
    {
        IParseError error = Fail("spawn --list vehicle");

        error.Code.Should().Be(ParseErrorCode.UnknownSubcommand);
        error.Message.Should().Contain("holds no subcommand");
    }

    /// <summary>
    ///     Verify that an error names the whole path the line walked, not only the last name on it.
    /// </summary>
    /// <remarks>
    ///     A name is only unique among its siblings, so an error about <c>entity</c> could be about any command
    ///     of that name; one about <c>spawn entity</c> is about the one that was typed.
    /// </remarks>
    [Theory]
    [InlineData("spawn entity")]
    [InlineData("spawn entity goblin 3 extra")]
    [InlineData("spawn entity goblin --colour red")]
    public void Parse_NamesTheWholePathInTheError(string input)
    {
        Fail(input).Message.Should().Contain("'spawn entity'");
    }

    /// <summary>
    ///     Reads the input, expecting it to fail.
    /// </summary>
    /// <param name="input">Input to read.</param>
    /// <returns>Returns the reason the parsing failed.</returns>
    private IParseError Fail(string input)
    {
        IParseResult result = _parser.Parse(input, Commands);

        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();

        return result.Error!;
    }
}

using AwesomeAssertions;
using Sagittaras.Commands;
using Sagittaras.Commands.Parsing;
using Sagittaras.Commands.Parsing.Tokens;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands.Parsing;

public class CommandParserTest
{
    private static readonly IReadOnlyList<ICommand> Commands = [new SpawnCommand(), new GodCommand(), new LoadSceneCommand()];

    private readonly CommandParser _parser = new();

    /// <summary>
    ///     Verify that the input is walked through the hierarchy down to the command it names.
    /// </summary>
    [Fact]
    public void Parse_WalksTheWholePath()
    {
        IParseResult result = Parse("spawn entity goblin");

        result.Success.Should().BeTrue();
        result.Command.Should().BeOfType<SpawnEntityCommand>();
    }

    /// <summary>
    ///     Verify that every token is left carrying the role it turned out to play.
    /// </summary>
    /// <remarks>
    ///     This is what a console highlighting the line reads, so the roles have to survive the parsing and
    ///     not only lead to the values.
    /// </remarks>
    [Fact]
    public void Parse_StampsTheRolesOntoTheTokens()
    {
        IParseResult result = Parse("spawn entity goblin --scene arena");

        result.Tokens.Select(token => token.Type)
            .Should()
            .Equal(TokenType.Command, TokenType.Command, TokenType.Argument, TokenType.Option, TokenType.OptionValue);
    }

    /// <summary>
    ///     Verify that positional arguments are bound in the order the command declares them.
    /// </summary>
    [Fact]
    public void Parse_BindsArgumentsInOrder()
    {
        IParseResult result = Parse("spawn entity goblin 3");

        result.Input!.GetValue<string>(SpawnEntityCommand.Entity).Should().Be("goblin");
        result.Input.GetValue<int>(SpawnEntityCommand.Count).Should().Be(3);
    }

    /// <summary>
    ///     Verify that an argument the input does not reach falls back to its own default.
    /// </summary>
    [Fact]
    public void Parse_FallsBackToTheDefaultOfAnArgument()
    {
        IParseResult result = Parse("spawn entity goblin");

        result.Input!.GetValue<int>(SpawnEntityCommand.Count).Should().Be(1);
        result.Input.IsSpecified(SpawnEntityCommand.Count).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that a value the input carried is told apart from one that was defaulted to.
    /// </summary>
    [Fact]
    public void Parse_MarksWhatTheInputNamed()
    {
        IParseResult result = Parse("spawn entity goblin 3");

        result.Input!.IsSpecified(SpawnEntityCommand.Count).Should().BeTrue();
    }

    /// <summary>
    ///     Verify that an option is bound by the name it is declared under.
    /// </summary>
    [Fact]
    public void Parse_BindsAnOptionByName()
    {
        IParseResult result = Parse("spawn entity goblin --scene arena");

        result.Input!.GetValue<string>(SpawnEntityCommand.Scene).Should().Be("arena");
    }

    /// <summary>
    ///     Verify that an option answers to its aliases as well.
    /// </summary>
    [Fact]
    public void Parse_BindsAnOptionByAlias()
    {
        IParseResult result = Parse("spawn entity goblin -s arena");

        result.Input!.GetValue<string>(SpawnEntityCommand.Scene).Should().Be("arena");
    }

    /// <summary>
    ///     Verify that an option may hand over its value through an equals sign.
    /// </summary>
    [Fact]
    public void Parse_BindsTheEqualsForm()
    {
        IParseResult result = Parse("spawn entity goblin --scene=arena");

        result.Input!.GetValue<string>(SpawnEntityCommand.Scene).Should().Be("arena");
    }

    /// <summary>
    ///     Verify that names are matched the way a console line is typed, which is without minding the case.
    /// </summary>
    [Fact]
    public void Parse_IgnoresTheCaseOfNames()
    {
        IParseResult result = Parse("SPAWN Entity goblin --SCENE arena");

        result.Command.Should().BeOfType<SpawnEntityCommand>();
        result.Input!.GetValue<string>(SpawnEntityCommand.Scene).Should().Be("arena");
    }

    /// <summary>
    ///     Verify that naming a flag is the whole of its value.
    /// </summary>
    [Fact]
    public void Parse_SetsAFlagByNamingIt()
    {
        IParseResult result = Parse("god --off");

        result.Input!.GetValue<bool>(GodCommand.Off).Should().BeTrue();
    }

    /// <summary>
    ///     Verify that a flag nobody names stays at its default and is reported as unspecified.
    /// </summary>
    [Fact]
    public void Parse_LeavesAnUnnamedFlagAtItsDefault()
    {
        IParseResult result = Parse("god");

        result.Input!.GetValue<bool>(GodCommand.Off).Should().BeFalse();
        result.Input.IsSpecified(GodCommand.Off).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that a flag still reads an explicit value handed to it through an equals sign.
    /// </summary>
    /// <remarks>
    ///     The pairing is spelled out in the input, so taking the value for a positional argument just
    ///     because a flag usually carries none would be reading the line wrong.
    /// </remarks>
    [Fact]
    public void Parse_ReadsAnExplicitValueOfAFlag()
    {
        IParseResult result = Parse("god --off=false");

        result.Input!.GetValue<bool>(GodCommand.Off).Should().BeFalse();
        result.Input.IsSpecified(GodCommand.Off).Should().BeTrue();
    }

    /// <summary>
    ///     Verify that an enum is read by the name of its member, ignoring the case.
    /// </summary>
    [Fact]
    public void Parse_ReadsAnEnumMember()
    {
        IParseResult result = Parse("spawn entity goblin --mode hostile");

        result.Input!.GetValue<SpawnMode>(SpawnEntityCommand.Mode).Should().Be(SpawnMode.Hostile);
    }

    /// <summary>
    ///     Verify that a negative number is taken for a value and not for an option.
    /// </summary>
    [Fact]
    public void Parse_TakesANegativeNumberForAValue()
    {
        IParseResult result = Parse("spawn entity goblin -3");

        result.Success.Should().BeTrue();
        result.Input!.GetValue<int>(SpawnEntityCommand.Count).Should().Be(-3);
    }

    /// <summary>
    ///     Verify that a branch of the hierarchy may be named alone, and resolves as itself.
    /// </summary>
    [Fact]
    public void Parse_ResolvesABranchNamedAlone()
    {
        IParseResult result = Parse("spawn");

        result.Success.Should().BeTrue();
        result.Command.Should().BeOfType<SpawnCommand>();
    }

    /// <summary>
    ///     Verify that a branch binds the options it declares itself.
    /// </summary>
    [Fact]
    public void Parse_BindsTheOptionsOfABranch()
    {
        IParseResult result = Parse("spawn --list");

        result.Command.Should().BeOfType<SpawnCommand>();
        result.Input!.GetValue<bool>(SpawnCommand.List).Should().BeTrue();
    }

    /// <summary>
    ///     Verify that a quoted value reaches the command as a single value.
    /// </summary>
    [Fact]
    public void Parse_BindsAQuotedValue()
    {
        IParseResult result = Parse("spawn item \"ancient guardian\"");

        result.Input!.GetValue<string>(SpawnItemCommand.Item).Should().Be("ancient guardian");
    }

    /// <summary>
    ///     Verify that an input the parser made sense of leaves nothing unaccounted for.
    /// </summary>
    [Fact]
    public void Parse_LeavesNothingUnmatchedOnSuccess()
    {
        IParseResult result = Parse("spawn entity goblin 3 --scene arena --dry-run");

        result.UnmatchedTokens.Should().BeEmpty();
        result.Error.Should().BeNull();
    }

    /// <summary>
    ///     Reads the input against the registered commands.
    /// </summary>
    /// <param name="input">Input to read.</param>
    /// <returns>Returns what the parser made of the input.</returns>
    private IParseResult Parse(string input)
    {
        return _parser.Parse(input, Commands);
    }
}

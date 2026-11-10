using AwesomeAssertions;
using Sagittaras.Commands;
using Sagittaras.Commands.Building;
using Sagittaras.Commands.Help;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parsing;
using Sagittaras.Commands.Parsing.Errors;
using Sagittaras.Commands.Parsing.Tokens;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands.Help;

public class HelpCommandTest
{
    private readonly SpawnCommand _spawn = new();

    private readonly ICommandManager _manager;

    public HelpCommandTest()
    {
        _manager = new CommandRegistryBuilder()
            .Add(_spawn)
            .Add(new GodCommand())
            .Add(new LoadSceneCommand())
            .WithHelp()
            .Build();
    }

    /// <summary>
    ///     Verify that the help named alone answers with the overview of every command, itself among them.
    /// </summary>
    [Fact]
    public void Help_AnswersWithTheOverview()
    {
        HelpOutput help = Help("help");

        help.Command.Should().BeNull();
        help.Path.Should().BeEmpty();
        help.Commands.Select(command => command.Name).Should().Equal("spawn", "god", "load", "help");
        help.ExitCode.Should().Be(0);
    }

    /// <summary>
    ///     Verify that the plain reading of the overview lists the commands with their descriptions lined up.
    /// </summary>
    [Fact]
    public void Help_WritesOutTheOverview()
    {
        Help("help").Content.Should().Be(Lines(
            "Available commands:",
            "  spawn  Spawns things into the active scene",
            "  god    Makes the player unkillable",
            "  load   Loads a scene",
            "  help   Describes the commands there are, or the one named behind it"));
    }

    /// <summary>
    ///     Verify that a help about a branch carries the registered branch itself and its subcommands.
    /// </summary>
    /// <remarks>
    ///     The help walks mirrors of the commands, but must never answer with a mirror - a console receiving
    ///     one would find none of the parameters of the command it stands for.
    /// </remarks>
    [Fact]
    public void Help_DescribesTheRegisteredBranch()
    {
        HelpOutput help = Help("help spawn");

        help.Command.Should().BeSameAs(_spawn);
        help.Path.Should().Equal(_spawn);
        help.Commands.Should().Equal(_spawn.Subcommands);
    }

    /// <summary>
    ///     Verify that the plain reading of a branch names its usage, its subcommands and its options.
    /// </summary>
    [Fact]
    public void Help_WritesOutABranch()
    {
        Help("help spawn").Content.Should().Be(Lines(
            "spawn - Spawns things into the active scene",
            "",
            "Usage: spawn <subcommand> [options]",
            "",
            "Subcommands:",
            "  entity  Spawns an entity into the active scene",
            "  item    Spawns an item on the ground",
            "",
            "Options:",
            "  --list, -l  List what can be spawned instead"));
    }

    /// <summary>
    ///     Verify that a help about a leaf carries the real command, with the path leading down to it.
    /// </summary>
    [Fact]
    public void Help_DescribesTheRegisteredLeaf()
    {
        HelpOutput help = Help("help spawn entity");

        help.Command.Should().BeOfType<SpawnEntityCommand>().Which.Arguments.Should().HaveCount(2);
        help.Path.Select(command => command.Name).Should().Equal("spawn", "entity");
        help.Commands.Should().BeEmpty();
    }

    /// <summary>
    ///     Verify that the plain reading of a leaf spells its usage from the top of the hierarchy, and explains
    ///     every argument and option with its default.
    /// </summary>
    /// <remarks>
    ///     A flag is off until named, so its default is not repeated; the defaults are spelled the way the
    ///     input would spell them.
    /// </remarks>
    [Fact]
    public void Help_WritesOutALeaf()
    {
        Help("help spawn entity").Content.Should().Be(Lines(
            "spawn entity - Spawns an entity into the active scene",
            "",
            "Usage: spawn entity <entity> [count] [options]",
            "",
            "Arguments:",
            "  entity  Entity to spawn",
            "  count   How many to spawn (default: 1)",
            "",
            "Options:",
            "  --scene, -s <value>  Scene to spawn into (default: active)",
            "  --mode <value>       Behaviour of the spawn (default: Idle)",
            "  --dry-run            Report the work without doing it"));
    }

    /// <summary>
    ///     Verify that the path behind the help is matched the way every line is, ignoring the case.
    /// </summary>
    [Fact]
    public void Help_IgnoresTheCaseOfThePath()
    {
        Help("HELP Spawn ENTITY").Command.Should().BeOfType<SpawnEntityCommand>();
    }

    /// <summary>
    ///     Verify that the path behind the help is stamped as commands, for a console to highlight.
    /// </summary>
    [Fact]
    public void Help_StampsThePathAsCommands()
    {
        _manager.Parse("help spawn entity").Tokens.Select(token => token.Type)
            .Should()
            .Equal(TokenType.Command, TokenType.Command, TokenType.Command);
    }

    /// <summary>
    ///     Verify that a name the hierarchy does not hold is refused, pointing at it.
    /// </summary>
    [Fact]
    public void Help_RefusesAnUnknownName()
    {
        RefusalOutput refusal = Refusal("help vehicle");

        refusal.Error.Code.Should().Be(ParseErrorCode.UnknownSubcommand);
        refusal.Error.Message.Should().Be("Command 'help' holds no subcommand named 'vehicle'.");
        refusal.Error.Token!.Value.Lexeme.Should().Be("vehicle");
    }

    /// <summary>
    ///     Verify that a name missing deeper in the hierarchy is refused with the path it was missing from.
    /// </summary>
    [Fact]
    public void Help_RefusesAnUnknownNameDeeper()
    {
        Refusal("help spawn vehicle").Error.Message.Should().Be("Command 'help spawn' holds no subcommand named 'vehicle'.");
    }

    /// <summary>
    ///     Verify that values behind the path are refused, and the refusal names the path of the help.
    /// </summary>
    /// <remarks>
    ///     A mirror bears the name of the command it mirrors but none of its arguments, so naming it alone
    ///     would claim that <c>entity</c> takes no arguments - true of the mirror, false of the command.
    /// </remarks>
    [Fact]
    public void Help_RefusesValuesBehindThePath()
    {
        IParseError error = Refusal("help spawn entity goblin").Error;

        error.Code.Should().Be(ParseErrorCode.TooManyArguments);
        error.Message.Should().StartWith("Command 'help spawn entity' takes 0 argument(s)");
    }

    /// <summary>
    ///     Verify that the help describes itself, without mirroring its own mirrors.
    /// </summary>
    [Fact]
    public void Help_DescribesItself()
    {
        HelpOutput help = Help("help help");

        help.Command.Should().BeOfType<HelpCommand>();
        help.Commands.Select(command => command.Name).Should().Equal("spawn", "god", "load", "help");
        Refusal("help help spawn").Error.Code.Should().Be(ParseErrorCode.TooManyArguments);
    }

    /// <summary>
    ///     Verify that the help is there only when asked for.
    /// </summary>
    [Fact]
    public void WithHelp_IsRequiredForTheHelp()
    {
        ICommandManager manager = new CommandRegistryBuilder().Add(new GodCommand()).Build();

        manager.Execute("help").Should().BeOfType<RefusalOutput>().Which.Error.Code.Should().Be(ParseErrorCode.UnknownCommand);
    }

    /// <summary>
    ///     Verify that the help describes the commands registered after it was asked for, too.
    /// </summary>
    [Fact]
    public void WithHelp_DescribesWhatIsRegisteredLater()
    {
        ICommandManager manager = new CommandRegistryBuilder().WithHelp().Add(new GodCommand()).Build();

        manager.Execute("help").Should().BeOfType<HelpOutput>().Which.Commands.Select(command => command.Name).Should().Equal("god", "help");
    }

    /// <summary>
    ///     Verify that the help is refused when a registered command already bears its name.
    /// </summary>
    [Fact]
    public void WithHelp_RefusesATakenName()
    {
        ICommandRegistryBuilder builder = new CommandRegistryBuilder().Add(new RunnableForeignCommand("HELP")).WithHelp();

        Action building = () => builder.Build();

        building.Should().Throw<InvalidOperationException>().WithMessage("*'help'*");
    }

    /// <summary>
    ///     Verify that a help about a command needs the path down to it.
    /// </summary>
    [Fact]
    public void About_RefusesAnEmptyPath()
    {
        Action describing = () => HelpOutput.About([]);

        describing.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    ///     Runs the line, expecting a help.
    /// </summary>
    private HelpOutput Help(string line)
    {
        return _manager.Execute(line).Should().BeOfType<HelpOutput>().Subject;
    }

    /// <summary>
    ///     Runs the line, expecting it to be refused.
    /// </summary>
    private RefusalOutput Refusal(string line)
    {
        return _manager.Execute(line).Should().BeOfType<RefusalOutput>().Subject;
    }

    /// <summary>
    ///     Joins the lines the way the plain reading does.
    /// </summary>
    private static string Lines(params string[] lines)
    {
        return string.Join(Environment.NewLine, lines);
    }
}

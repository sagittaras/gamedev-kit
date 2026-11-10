using AwesomeAssertions;
using Sagittaras.Commands;
using Sagittaras.Commands.Building;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parameters;
using Sagittaras.Commands.Parsing;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands.Building;

public class CommandRegistryBuilderTest
{
    private readonly CommandRegistryBuilder _builder = new();

    /// <summary>
    ///     Verify that the built manager holds the commands the first token is resolved against.
    /// </summary>
    [Fact]
    public void Build_HandsTheCommandsToTheManager()
    {
        ICommandManager manager = _builder.Add(new SpawnCommand()).Add(new GodCommand()).Build();

        manager.Commands.Select(command => command.Name).Should().Equal("spawn", "god");
    }

    /// <summary>
    ///     Verify that the registrations are copied, so a later one does not reach a manager already built.
    /// </summary>
    [Fact]
    public void Build_CopiesTheRegistrations()
    {
        ICommandManager manager = _builder.Add(new SpawnCommand()).Build();

        _builder.Add(new GodCommand());

        manager.Commands.Should().HaveCount(1);
    }

    /// <summary>
    ///     Verify that two commands answering to the same name are refused, case or no case.
    /// </summary>
    /// <remarks>
    ///     The input is matched ignoring the case, so a name differing only by it would leave one of the two
    ///     unreachable.
    /// </remarks>
    [Fact]
    public void Add_RefusesTwoCommandsOfTheSameName()
    {
        _builder.Add(new RunnableForeignCommand("spawn"));

        Action adding = () => _builder.Add(new RunnableForeignCommand("SPAWN"));

        adding.Should().Throw<InvalidOperationException>().WithMessage("*already registered*");
    }

    /// <summary>
    ///     Verify that a command nobody could run is refused, although the parser could resolve it.
    /// </summary>
    /// <remarks>
    ///     The bases of the package cannot be written this way, but the contract can be implemented by hand -
    ///     and a command that resolves and then does nothing is worth hearing about during the boot.
    /// </remarks>
    [Fact]
    public void Add_RefusesACommandThatCannotBeRun()
    {
        Action adding = () => _builder.Add(new ForeignCommand("spawn"));

        adding.Should().Throw<InvalidOperationException>().WithMessage("*ISyncCommand*");
    }

    /// <summary>
    ///     Verify that subcommands next to positional arguments are refused.
    /// </summary>
    [Fact]
    public void Add_RefusesSubcommandsNextToArguments()
    {
        ICommand command = new RunnableForeignCommand("spawn", [new RunnableForeignCommand("entity")], [SpawnItemCommand.Item]);

        Action adding = () => _builder.Add(command);

        adding.Should().Throw<InvalidOperationException>().WithMessage("*subcommands next to positional arguments*");
    }

    /// <summary>
    ///     Verify that two subcommands of the same name are refused.
    /// </summary>
    [Fact]
    public void Add_RefusesTwoSubcommandsOfTheSameName()
    {
        ICommand command = new RunnableForeignCommand("spawn", [new RunnableForeignCommand("entity"), new RunnableForeignCommand("entity")]);

        Action adding = () => _builder.Add(command);

        adding.Should().Throw<InvalidOperationException>().WithMessage("*two subcommands*");
    }

    /// <summary>
    ///     Verify that the checks reach the whole subtree and not only what was handed over.
    /// </summary>
    [Fact]
    public void Add_ChecksOverTheWholeSubtree()
    {
        ICommand broken = new ForeignCommand("entity");
        ICommand branch = new RunnableForeignCommand("spawn", [new RunnableForeignCommand("item", [broken])]);

        Action adding = () => _builder.Add(branch);

        adding.Should().Throw<InvalidOperationException>().WithMessage("*entity*");
    }

    /// <summary>
    ///     Verify that a required argument behind an optional one is refused, naming both.
    /// </summary>
    /// <remarks>
    ///     Values go to the arguments in the order they are declared, so the optional one would always take the
    ///     value meant for the required one, which would then be reported missing on a line holding everything.
    /// </remarks>
    [Fact]
    public void Add_RefusesARequiredArgumentBehindAnOptionalOne()
    {
        ICommand command = new RunnableForeignCommand("spawn", arguments: [new Argument<int>("count", "How many", 1), new Argument<string>("entity", "What")]);

        Action adding = () => _builder.Add(command);

        adding.Should().Throw<InvalidOperationException>().WithMessage("*'entity' behind the optional 'count'*");
    }

    /// <summary>
    ///     Verify that a command written through the bases of the package is checked the same way.
    /// </summary>
    /// <remarks>
    ///     The bases keep a command from holding subcommands next to arguments, but not the order of the
    ///     arguments - that is left to the registration, like every other check a whole command is needed for.
    /// </remarks>
    [Fact]
    public void Add_RefusesTheOrderInACommandOfThePackage()
    {
        Action adding = () => _builder.Add(new MisorderedCommand());

        adding.Should().Throw<InvalidOperationException>().WithMessage("*'entity' behind the optional 'count'*");
    }

    /// <summary>
    ///     Verify that optional arguments behind the required ones pass, however many of them there are.
    /// </summary>
    [Fact]
    public void Add_AcceptsOptionalArgumentsLast()
    {
        ICommand command = new RunnableForeignCommand("spawn", arguments: [new Argument<string>("entity", "What"), new Argument<int>("count", "How many", 1), new Argument<int>("level", "Which", 1)]);

        Action adding = () => _builder.Add(command);

        adding.Should().NotThrow();
    }

    /// <summary>
    ///     Verify that the order is checked deeper in the hierarchy as well.
    /// </summary>
    [Fact]
    public void Add_ChecksTheOrderOverTheWholeSubtree()
    {
        ICommand leaf = new RunnableForeignCommand("entity", arguments: [new Argument<int>("count", "How many", 1), new Argument<string>("entity", "What")]);

        Action adding = () => _builder.Add(new RunnableForeignCommand("spawn", [leaf]));

        adding.Should().Throw<InvalidOperationException>().WithMessage("*'entity' behind the optional 'count'*");
    }

    /// <summary>
    ///     Verify that a command built by the bases of the package passes the checks as it is.
    /// </summary>
    [Fact]
    public void Add_AcceptsTheHierarchyOfThePackage()
    {
        Action adding = () => _builder.Add(new SpawnCommand()).Add(new GodCommand()).Add(new LoadSceneCommand());

        adding.Should().NotThrow();
    }

    /// <summary>
    ///     Verify that the manager reads its input through the parser it was given.
    /// </summary>
    /// <remarks>
    ///     The seam another syntax arrives through, and the one a converter of the game's own types reaches
    ///     the binding by.
    /// </remarks>
    [Fact]
    public void WithParser_IsUsedByTheManager()
    {
        RecordingParser parser = new();
        ICommandManager manager = _builder.Add(new GodCommand()).WithParser(parser).Build();

        manager.Parse("god");

        parser.Inputs.Should().Equal("god");
    }

    /// <summary>
    ///     Parser remembering what it was asked to read, in place of the default one.
    /// </summary>
    private class RecordingParser : ICommandParser
    {
        public List<string> Inputs { get; } = new();

        public IParseResult Parse(string input, IReadOnlyList<ICommand> commands)
        {
            Inputs.Add(input);

            return new CommandParser().Parse(input, commands);
        }
    }

    /// <summary>
    ///     Command of the package declaring its optional argument in front of the required one.
    /// </summary>
    private class MisorderedCommand : Command
    {
        private static readonly Argument<int> Count = new("count", "How many", 1);

        private static readonly Argument<string> Entity = new("entity", "What");

        public MisorderedCommand()
            : base("misordered", "Declares its arguments the wrong way round")
        {
            Accepts(Count, Entity);
        }

        public override ICommandOutput Invoke(ICommandInput input)
        {
            return CommandOutput.Success();
        }
    }
}

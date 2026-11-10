using AwesomeAssertions;
using Sagittaras.Commands;
using Sagittaras.Commands.Building;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parsing;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands;

public class CommandManagerTest
{
    private readonly CountingCommand _counting = new();

    private readonly ICommandManager _manager;

    public CommandManagerTest()
    {
        _manager = new CommandRegistryBuilder()
            .Add(new SpawnCommand())
            .Add(new GodCommand())
            .Add(new LoadSceneCommand())
            .Add(new BrokenCommand())
            .Add(_counting)
            .Build();
    }

    /// <summary>
    ///     Verify that a line of input reaches the command it names and answers with what it returned.
    /// </summary>
    [Fact]
    public void Execute_RunsWhatTheInputResolvedTo()
    {
        ICommandOutput output = _manager.Execute("spawn entity goblin 3 --scene arena");

        output.ExitCode.Should().Be(0);
        output.Content.Should().Be("3x goblin in arena");
    }

    /// <summary>
    ///     Verify that an input the parser could not read is answered with an output and not an exception.
    /// </summary>
    /// <remarks>
    ///     A console has somewhere to print either, and a typo is not worth a stack trace.
    /// </remarks>
    [Fact]
    public void Execute_AnswersAnUnreadableInputWithAnOutput()
    {
        ICommandOutput output = _manager.Execute("teleport 3 4");

        output.ExitCode.Should().Be(RefusalOutput.UsageExitCode);
        output.Content.Should().Contain("teleport");
    }

    /// <summary>
    ///     Verify that a mistyped line is told apart from work that did not go through.
    /// </summary>
    /// <remarks>
    ///     Both fail, and a console reacts to them differently - one is answered by showing the usage, the
    ///     other by showing what went wrong.
    /// </remarks>
    [Fact]
    public void Execute_TellsAUsageErrorFromAFailingCommand()
    {
        _manager.Execute("broken").ExitCode.Should().Be(3);
        _manager.Execute("brokn").ExitCode.Should().Be(RefusalOutput.UsageExitCode);
    }

    /// <summary>
    ///     Verify that a branch named without a subcommand answers with what it holds.
    /// </summary>
    [Fact]
    public void Execute_AnswersABranchWithItsUsage()
    {
        ICommandOutput output = _manager.Execute("spawn");

        output.ExitCode.Should().NotBe(0);
        output.Content.Should().Contain("entity").And.Contain("item");
    }

    /// <summary>
    ///     Verify that a command which has to be awaited is refused by the synchronous entrypoint.
    /// </summary>
    /// <remarks>
    ///     Blocking on it would deadlock on the very thread a console tends to live on, so it is refused
    ///     outright rather than waited for.
    /// </remarks>
    [Fact]
    public void Execute_RefusesAnAwaitedCommand()
    {
        Action running = () => _manager.Execute("load arena");

        running.Should().Throw<InvalidOperationException>().WithMessage("*ExecuteAsync*");
    }

    /// <summary>
    ///     Verify that the awaiting entrypoint reaches a command that has to be awaited.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_AwaitsAnAsynchronousCommand()
    {
        ICommandOutput output = await _manager.ExecuteAsync("load arena");

        output.Content.Should().Be("loaded arena");
    }

    /// <summary>
    ///     Verify that the awaiting entrypoint reaches a synchronous command just as well.
    /// </summary>
    /// <remarks>
    ///     Which is what lets a console with an awaiting loop need nothing else.
    /// </remarks>
    [Fact]
    public async Task ExecuteAsync_ReachesASynchronousCommand()
    {
        ICommandOutput output = await _manager.ExecuteAsync("god --off");

        output.Content.Should().Be("god off");
    }

    /// <summary>
    ///     Verify that the awaiting entrypoint answers an unreadable input the same way as the plain one.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_AnswersAnUnreadableInputWithAnOutput()
    {
        ICommandOutput output = await _manager.ExecuteAsync("teleport");

        output.ExitCode.Should().Be(RefusalOutput.UsageExitCode);
    }

    /// <summary>
    ///     Verify that reading an input runs nothing at all.
    /// </summary>
    /// <remarks>
    ///     What lets a console show what a line would do before committing to it.
    /// </remarks>
    [Fact]
    public void Parse_RunsNothing()
    {
        IParseResult result = _manager.Parse("counting");

        result.Success.Should().BeTrue();
        _counting.Invocations.Should().Be(0);
    }

    /// <summary>
    ///     Verify that an already parsed input can be run without being read a second time.
    /// </summary>
    [Fact]
    public void Execute_RunsAnAlreadyParsedInput()
    {
        IParseResult result = _manager.Parse("counting");

        _manager.Execute(result);

        _counting.Invocations.Should().Be(1);
    }

    /// <summary>
    ///     Command counting how many times it was actually run.
    /// </summary>
    private class CountingCommand : Command
    {
        public CountingCommand()
            : base("counting", "Counts how many times it was run")
        {
        }

        public int Invocations { get; private set; }

        public override ICommandOutput Invoke(ICommandInput input)
        {
            Invocations++;

            return CommandOutput.Success(Invocations.ToString());
        }
    }
}

using AwesomeAssertions;
using Sagittaras.Commands;
using Sagittaras.Commands.Building;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parsing;
using Sagittaras.Commands.Parsing.Errors;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands.Parsing;

public class RefusalOutputTest
{
    private static readonly IReadOnlyList<ICommand> Commands = [new SpawnCommand()];

    private readonly ICommandManager _manager = new CommandRegistryBuilder().Add(new SpawnCommand()).Build();

    /// <summary>
    ///     Verify that the refusal carries the whole failed parsing and the error within it.
    /// </summary>
    [Fact]
    public void Result_IsTheFailedParsing()
    {
        IParseResult result = new CommandParser().Parse("teleport", Commands);

        RefusalOutput refusal = new(result);

        refusal.Result.Should().BeSameAs(result);
        refusal.Error.Code.Should().Be(ParseErrorCode.UnknownCommand);
    }

    /// <summary>
    ///     Verify that the plain reading is the message of the error, and the exit code the one of a mistyped line.
    /// </summary>
    [Fact]
    public void Content_IsTheMessageOfTheError()
    {
        IParseResult result = new CommandParser().Parse("teleport", Commands);

        RefusalOutput refusal = new(result);

        refusal.Content.Should().Be(result.Error!.Message);
        refusal.ExitCode.Should().Be(RefusalOutput.UsageExitCode);
    }

    /// <summary>
    ///     Verify that a parsing that went through is not something to refuse.
    /// </summary>
    [Fact]
    public void Constructor_RefusesASuccessfulParsing()
    {
        IParseResult result = new CommandParser().Parse("spawn", Commands);

        Action refusing = () => _ = new RefusalOutput(result);

        refusing.Should().Throw<ArgumentException>();
    }

    /// <summary>
    ///     Verify that an unreadable line answers with a refusal a console can point at the wrong word with.
    /// </summary>
    /// <remarks>
    ///     The reason the refusal carries the parsing: the token and the command the line got as far as are
    ///     there without the console having to parse the line a second time.
    /// </remarks>
    [Fact]
    public void Execute_LetsTheConsolePointAtTheWrongWord()
    {
        ICommandOutput output = _manager.Execute("spawn vehicle");

        RefusalOutput refusal = output.Should().BeOfType<RefusalOutput>().Subject;
        refusal.Error.Token!.Value.Lexeme.Should().Be("vehicle");
        refusal.Error.Token!.Value.Start.Should().Be(6);
        refusal.Result.Command.Should().BeOfType<SpawnCommand>();
    }

    /// <summary>
    ///     Verify that the awaiting entrypoint refuses the same way.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_AnswersWithARefusalToo()
    {
        ICommandOutput output = await _manager.ExecuteAsync("spawn vehicle");

        output.Should().BeOfType<RefusalOutput>();
    }
}

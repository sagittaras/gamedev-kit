using AwesomeAssertions;
using Sagittaras.Commands;
using Sagittaras.Commands.Building;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parsing;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands.Invocation;

public class UsageOutputTest
{
    private readonly SpawnCommand _branch = new();

    /// <summary>
    ///     Verify that the usage carries the branch itself, for a console to lay out a help of its own.
    /// </summary>
    [Fact]
    public void Command_IsTheBranchItself()
    {
        new UsageOutput(_branch).Command.Should().BeSameAs(_branch);
    }

    /// <summary>
    ///     Verify that the plain reading lists the subcommands with their descriptions lined up.
    /// </summary>
    /// <remarks>
    ///     What a console that knows nothing of the type prints, so it has to stand on its own.
    /// </remarks>
    [Fact]
    public void Content_ListsTheSubcommandsLinedUp()
    {
        string expected = string.Join(Environment.NewLine,
            "Command 'spawn' has to be followed by one of:",
            "  entity  Spawns an entity into the active scene",
            "  item    Spawns an item on the ground");

        new UsageOutput(_branch).Content.Should().Be(expected);
    }

    /// <summary>
    ///     Verify that a branch named alone is not a success, and not a mistake of the input either.
    /// </summary>
    [Fact]
    public void ExitCode_IsTheOneOfABranchNamedAlone()
    {
        int exitCode = new UsageOutput(_branch).ExitCode;

        exitCode.Should().Be(UsageOutput.BranchExitCode);
        exitCode.Should().NotBe(0).And.NotBe(RefusalOutput.UsageExitCode);
    }

    /// <summary>
    ///     Verify that a branch named alone answers with its usage, and the console can tell it by the type.
    /// </summary>
    [Fact]
    public void Execute_AnswersABranchWithItsUsage()
    {
        ICommandManager manager = new CommandRegistryBuilder().Add(new SpawnCommand()).Build();

        ICommandOutput output = manager.Execute("spawn");

        output.Should().BeOfType<UsageOutput>().Which.Command.Should().BeOfType<SpawnCommand>();
    }
}

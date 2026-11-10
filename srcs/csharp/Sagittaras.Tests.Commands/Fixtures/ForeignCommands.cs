using Sagittaras.Commands;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parameters;

namespace Sagittaras.Tests.Commands.Fixtures;

/// <summary>
///     Command written without the bases of the package, to reach the checks the builder makes.
/// </summary>
/// <remarks>
///     The bases keep a command from being built wrongly, so the only way to hand the builder something
///     broken is to implement the contract by hand - which anybody outside the package may do.
/// </remarks>
public class ForeignCommand(string name, IReadOnlyList<ICommand>? subcommands = null, IReadOnlyList<IArgument>? arguments = null) : ICommand
{
    public string Name { get; } = name;

    public string Description => "Written without the bases of the package";

    public IReadOnlyList<ICommand> Subcommands { get; } = subcommands ?? [];

    public IReadOnlyList<IArgument> Arguments { get; } = arguments ?? [];

    public IReadOnlyList<IOption> Options => [];
}

/// <summary>
///     Foreign command that can actually be run.
/// </summary>
public class RunnableForeignCommand(string name, IReadOnlyList<ICommand>? subcommands = null, IReadOnlyList<IArgument>? arguments = null)
    : ForeignCommand(name, subcommands, arguments), ISyncCommand
{
    public ICommandOutput Invoke(ICommandInput input)
    {
        return CommandOutput.Success(Name);
    }
}

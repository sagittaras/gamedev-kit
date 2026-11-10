using System.Globalization;
using Sagittaras.Commands;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parameters;

namespace Sagittaras.Tests.Commands.Fixtures;

/// <summary>
///     Enum spelled by an option, to have a value that is neither a number nor a string.
/// </summary>
public enum SpawnMode
{
    Idle,
    Hostile,
}

/// <summary>
///     Leaf carrying one of everything: a required argument, an optional one, options and a flag.
/// </summary>
public class SpawnEntityCommand : Command
{
    public static readonly Argument<string> Entity = new("entity", "Entity to spawn");

    public static readonly Argument<int> Count = new("count", "How many to spawn", 1);

    public static readonly Option<string> Scene = new("scene", "Scene to spawn into", "active", "s");

    public static readonly Option<SpawnMode> Mode = new("mode", "Behaviour of the spawn", SpawnMode.Idle);

    public static readonly Flag DryRun = new("dry-run", "Report the work without doing it");

    public SpawnEntityCommand()
        : base("entity", "Spawns an entity into the active scene")
    {
        Accepts(Entity, Count, Scene, Mode, DryRun);
    }

    public override ICommandOutput Invoke(ICommandInput input)
    {
        return CommandOutput.Success($"{input.GetValue<int>(Count)}x {input.GetValue<string>(Entity)} in {input.GetValue<string>(Scene)}");
    }
}

/// <summary>
///     Leaf taking a single required argument.
/// </summary>
public class SpawnItemCommand : Command
{
    public static readonly Argument<string> Item = new("item", "Item to spawn");

    public SpawnItemCommand()
        : base("item", "Spawns an item on the ground")
    {
        Accepts(Item);
    }

    public override ICommandOutput Invoke(ICommandInput input)
    {
        return CommandOutput.Success(input.GetValue<string>(Item));
    }
}

/// <summary>
///     Branch of the hierarchy, with a flag of its own to prove options survive next to subcommands.
/// </summary>
public class SpawnCommand : CommandGroup
{
    public static readonly Flag List = new("list", "List what can be spawned instead", "l");

    public SpawnCommand()
        : base("spawn", "Spawns things into the active scene")
    {
        Accepts(List);
        Add(new SpawnEntityCommand());
        Add(new SpawnItemCommand());
    }
}

/// <summary>
///     Top-level leaf taking nothing at all.
/// </summary>
public class GodCommand : Command
{
    public static readonly Flag Off = new("off", "Turn the mode off instead of on");

    public GodCommand()
        : base("god", "Makes the player unkillable")
    {
        Accepts(Off);
    }

    public override ICommandOutput Invoke(ICommandInput input)
    {
        return CommandOutput.Success(input.GetValue<bool>(Off) ? "god off" : "god on");
    }
}

/// <summary>
///     Leaf whose work has to be awaited.
/// </summary>
public class LoadSceneCommand : AsyncCommand
{
    public static readonly Argument<string> Scene = new("scene", "Scene to load");

    public LoadSceneCommand()
        : base("load", "Loads a scene")
    {
        Accepts(Scene);
    }

    public override async ValueTask<ICommandOutput> InvokeAsync(ICommandInput input, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return CommandOutput.Success($"loaded {input.GetValue<string>(Scene)}");
    }
}

/// <summary>
///     Leaf answering with a failure, to have a non-zero exit code that is not a usage error.
/// </summary>
public class BrokenCommand : Command
{
    public BrokenCommand()
        : base("broken", "Fails at what it does")
    {
    }

    public override ICommandOutput Invoke(ICommandInput input)
    {
        return CommandOutput.Failure("the scene is not loaded", 3);
    }
}

/// <summary>
///     Leaf whose parameters read their values their own way.
/// </summary>
/// <remarks>
///     The share reads words the converter would never read as an integer, and holds numbers to a range no
///     integer type has; the target accepts only the names there is somebody to heal under.
/// </remarks>
public class HealCommand : Command
{
    public static readonly Argument<int> Percent = new("percent", "Share of the health to restore")
    {
        Parser = ReadPercent,
    };

    public static readonly Option<string> Target = new("target", "Who gets healed", "player", "t")
    {
        Parser = text => text is "player" or "companion"
            ? ParsedValue<string>.Read(text)
            : ParsedValue<string>.Refused($"There is nobody called '{text}' to heal."),
    };

    public HealCommand()
        : base("heal", "Restores a share of the health")
    {
        Accepts(Percent, Target);
    }

    public override ICommandOutput Invoke(ICommandInput input)
    {
        return CommandOutput.Success($"{input.GetValue<string>(Target)} healed by {input.GetValue<int>(Percent)}%");
    }

    private static ParsedValue<int> ReadPercent(string text)
    {
        return text switch
        {
            "half" => ParsedValue<int>.Read(50),
            "full" => ParsedValue<int>.Read(100),
            _ when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && value is >= 0 and <= 100 => ParsedValue<int>.Read(value),
            _ => ParsedValue<int>.Refused($"The share has to be between 0 and 100, and '{text}' is not."),
        };
    }
}

/// <summary>
///     Leaf whose parameters judge the values once they are read.
/// </summary>
/// <remarks>
///     The count is read by the converter and only held to a range; the creature is read by a parser that
///     knows a drake for a dragon, and judged by a validator that will not have dragons - so a drake is
///     refused only if the validator sees what the parser made of the text.
/// </remarks>
public class SummonCommand : Command
{
    public static readonly Argument<int> Count = new("count", "How many to summon", 1)
    {
        Validator = count => count is >= 1 and <= 10 ? null : $"Between 1 and 10 can be summoned at once, not {count}.",
    };

    public static readonly Option<string> Creature = new("creature", "What to summon", "imp", "c")
    {
        Parser = text => ParsedValue<string>.Read(text == "drake" ? "dragon" : text),
        Validator = creature => creature == "dragon" ? "Dragons cannot be summoned." : null,
    };

    public SummonCommand()
        : base("summon", "Summons creatures to fight alongside")
    {
        Accepts(Count, Creature);
    }

    public override ICommandOutput Invoke(ICommandInput input)
    {
        return CommandOutput.Success($"{input.GetValue<int>(Count)}x {input.GetValue<string>(Creature)}");
    }
}

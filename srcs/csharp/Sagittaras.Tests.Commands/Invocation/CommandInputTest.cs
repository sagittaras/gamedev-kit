using AwesomeAssertions;
using Sagittaras.Commands;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parameters;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands.Invocation;

public class CommandInputTest
{
    private readonly SpawnEntityCommand _command = new();

    /// <summary>
    ///     Verify that an input can be composed without a parser ever having been involved.
    /// </summary>
    /// <remarks>
    ///     This is what keeps a command reachable from a button in the editor tooling or from a startup
    ///     script - the command sees the same thing either way.
    /// </remarks>
    [Fact]
    public void Compose_BindsTheGivenValues()
    {
        ICommandInput input = Compose(new Dictionary<IParameter, object?>
        {
            [SpawnEntityCommand.Entity] = "goblin",
            [SpawnEntityCommand.Count] = 3,
        });

        input.GetValue<string>(SpawnEntityCommand.Entity).Should().Be("goblin");
        input.GetValue<int>(SpawnEntityCommand.Count).Should().Be(3);
    }

    /// <summary>
    ///     Verify that a parameter left out falls back to its own default.
    /// </summary>
    [Fact]
    public void Compose_FallsBackToTheDefaults()
    {
        ICommandInput input = Compose(new Dictionary<IParameter, object?> { [SpawnEntityCommand.Entity] = "goblin" });

        input.GetValue<int>(SpawnEntityCommand.Count).Should().Be(1);
        input.GetValue<string>(SpawnEntityCommand.Scene).Should().Be("active");
        input.GetValue<bool>(SpawnEntityCommand.DryRun).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that what was handed over is told apart from what was defaulted to.
    /// </summary>
    [Fact]
    public void Compose_MarksWhatWasHandedOver()
    {
        ICommandInput input = Compose(new Dictionary<IParameter, object?> { [SpawnEntityCommand.Entity] = "goblin" });

        input.IsSpecified(SpawnEntityCommand.Entity).Should().BeTrue();
        input.IsSpecified(SpawnEntityCommand.Count).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that an input composed without a required argument is refused.
    /// </summary>
    /// <remarks>
    ///     A command run over it would have nothing to read, and finding that out inside the invocation is
    ///     late.
    /// </remarks>
    [Fact]
    public void Compose_RefusesAMissingRequiredArgument()
    {
        Action composing = () => Compose(new Dictionary<IParameter, object?>());

        composing.Should().Throw<ArgumentException>().WithMessage("*entity*");
    }

    /// <summary>
    ///     Verify that a value meant for a parameter of another command is refused.
    /// </summary>
    /// <remarks>
    ///     Most often a parameter that was renamed, or one belonging to a sibling command - either way the
    ///     value would never be read, and silence about it hides the mistake.
    /// </remarks>
    [Fact]
    public void Compose_RefusesAnUndeclaredParameter()
    {
        Action composing = () => Compose(new Dictionary<IParameter, object?>
        {
            [SpawnEntityCommand.Entity] = "goblin",
            [SpawnItemCommand.Item] = "sword",
        });

        composing.Should().Throw<ArgumentException>().WithMessage("*item*");
    }

    /// <summary>
    ///     Verify that reading a parameter the input was not built for is refused rather than defaulted.
    /// </summary>
    [Fact]
    public void GetValue_RefusesAParameterItWasNotBuiltFor()
    {
        ICommandInput input = Compose(new Dictionary<IParameter, object?> { [SpawnEntityCommand.Entity] = "goblin" });

        Action reading = () => input.GetValue<string>(SpawnItemCommand.Item);

        reading.Should().Throw<ArgumentException>();
    }

    /// <summary>
    ///     Verify that the trying overload answers with false where reading would throw.
    /// </summary>
    [Fact]
    public void TryGetValue_AnswersWithoutThrowing()
    {
        ICommandInput input = Compose(new Dictionary<IParameter, object?> { [SpawnEntityCommand.Entity] = "goblin" });

        input.TryGetValue(SpawnItemCommand.Item, out string? missing).Should().BeFalse();
        missing.Should().BeNull();

        input.TryGetValue(SpawnEntityCommand.Entity, out string? found).Should().BeTrue();
        found.Should().Be("goblin");
    }

    /// <summary>
    ///     Verify that a composed value is held to the validator of its parameter, in the validator's words.
    /// </summary>
    /// <remarks>
    ///     A command has to be able to rely on its validator wherever the value came from - a button in the
    ///     editor tooling must not reach it with a value the console would have refused.
    /// </remarks>
    [Fact]
    public void Compose_RefusesAValueTheValidatorRefuses()
    {
        Action composing = () => new CommandInput(new SummonCommand(), new Dictionary<IParameter, object?> { [SummonCommand.Count] = 50 });

        composing.Should().Throw<ArgumentException>().WithMessage("Between 1 and 10 can be summoned at once, not 50.*");
    }

    /// <summary>
    ///     Verify that a composed value the validator accepts is bound.
    /// </summary>
    [Fact]
    public void Compose_TakesAValueTheValidatorAccepts()
    {
        ICommandInput input = new CommandInput(new SummonCommand(), new Dictionary<IParameter, object?> { [SummonCommand.Count] = 5 });

        input.GetValue<int>(SummonCommand.Count).Should().Be(5);
    }

    /// <summary>
    ///     Verify that a default is not judged, as nobody gave it.
    /// </summary>
    [Fact]
    public void Compose_LeavesTheDefaultUnjudged()
    {
        ICommandInput input = new CommandInput(new OddCommand(), new Dictionary<IParameter, object?>());

        input.GetValue<int>(OddCommand.Level).Should().Be(0);
    }

    /// <summary>
    ///     Verify that a value of another type than the declared one is refused plainly.
    /// </summary>
    /// <remarks>
    ///     Without the check it would reach the validator, which judges a value of the declared type, and
    ///     break it with a cast nobody could read.
    /// </remarks>
    [Fact]
    public void Compose_RefusesAValueOfAnotherType()
    {
        Action composing = () => Compose(new Dictionary<IParameter, object?>
        {
            [SpawnEntityCommand.Entity] = "goblin",
            [SpawnEntityCommand.Count] = "three",
        });

        composing.Should().Throw<ArgumentException>().WithMessage("*type Int32*");
    }

    /// <summary>
    ///     Verify that null is refused where the declared type cannot hold it.
    /// </summary>
    [Fact]
    public void Compose_RefusesNullForAValueType()
    {
        Action composing = () => Compose(new Dictionary<IParameter, object?>
        {
            [SpawnEntityCommand.Entity] = "goblin",
            [SpawnEntityCommand.Count] = null,
        });

        composing.Should().Throw<ArgumentException>().WithMessage("*type Int32*");
    }

    /// <summary>
    ///     Verify that null is taken where the declared type can hold it.
    /// </summary>
    [Fact]
    public void Compose_TakesNullWhereTheTypeAllowsIt()
    {
        ICommandInput input = Compose(new Dictionary<IParameter, object?>
        {
            [SpawnEntityCommand.Entity] = "goblin",
            [SpawnEntityCommand.Scene] = null,
        });

        input.GetValue<string?>(SpawnEntityCommand.Scene).Should().BeNull();
    }

    /// <summary>
    ///     Verify that a value of the type a nullable parameter wraps is taken as that parameter's value.
    /// </summary>
    [Fact]
    public void Compose_TakesAValueOfANullableType()
    {
        ICommandInput input = new CommandInput(new OddCommand(), new Dictionary<IParameter, object?> { [OddCommand.Chance] = 5 });

        input.GetValue<int?>(OddCommand.Chance).Should().Be(5);
    }

    /// <summary>
    ///     Verify that a refusal given in blank words is still answered, and plainly.
    /// </summary>
    [Fact]
    public void Compose_AnswersABlankRefusalPlainly()
    {
        Action composing = () => new CommandInput(new OddCommand(), new Dictionary<IParameter, object?> { [OddCommand.Mood] = "grumpy" });

        composing.Should().Throw<ArgumentException>().WithMessage("Parameter 'mood' does not accept 'grumpy'.*");
    }

    /// <summary>
    ///     Composes an input for the command out of the given values.
    /// </summary>
    /// <param name="values">Values known for the parameters of the command.</param>
    /// <returns>Returns the composed input.</returns>
    private ICommandInput Compose(IReadOnlyDictionary<IParameter, object?> values)
    {
        return new CommandInput(_command, values);
    }

    /// <summary>
    ///     Command with a default its own validator would refuse, a nullable parameter, and a validator refusing
    ///     in blank words.
    /// </summary>
    private class OddCommand : Command
    {
        public static readonly Argument<int> Level = new("level", "Level to be odd at", 0)
        {
            Validator = level => level > 0 ? null : "The level has to be above zero.",
        };

        public static readonly Argument<int?> Chance = new("chance", "Chance of being odd", null);

        public static readonly Option<string> Mood = new("mood", "Mood to be odd in", "calm")
        {
            Validator = mood => mood == "grumpy" ? "  " : null,
        };

        public OddCommand()
            : base("odd", "Behaves oddly")
        {
            Accepts(Level, Chance, Mood);
        }

        public override ICommandOutput Invoke(ICommandInput input)
        {
            return CommandOutput.Success();
        }
    }
}

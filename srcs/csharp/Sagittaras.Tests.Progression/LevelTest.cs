using AwesomeAssertions;
using Sagittaras.Progression;
using Sagittaras.Progression.Formulas;

namespace Sagittaras.Tests.Progression;

public class LevelTest
{
    /// <summary>
    ///     Verify behavior of <see cref="Level.Gain"/> for single level.
    /// </summary>
    [Fact]
    public void Gain_SingleLevel()
    {
        const int overflow = 50;
        
        ExponentialScalingFormula formula = new();
        Level level = new(1, formula);
        
        LevelGainResult result = level.Gain(formula.Calculate(1) + overflow);
        result.LevelsGained.Should().Be(1);
        result.LeveledUp.Should().BeTrue();

        Level current = result;
        current.Progress.Current.Should().Be(overflow);
    }

    /// <summary>
    ///     Verify behavior of <see cref="Level.Gain"/> for multiple levels at once.
    /// </summary>
    [Fact]
    public void Gain_MultipleLevels()
    {
        const int levelsToGain = 4;
        
        ExponentialScalingFormula formula = new();
        Level level = new(1, formula);
        Experience toGain = Experience.Zero;

        for (int i = 0; i < levelsToGain; i++)
        {
            toGain += formula.Calculate(i + 1);
        }

        LevelGainResult result = level.Gain(toGain);
        result.LevelsGained.Should().Be(levelsToGain);
        result.LeveledUp.Should().BeTrue();

        Level current = result;
        current.Value.Should().Be(levelsToGain + 1);
        current.Progress.Current.Should().Be(Experience.Zero);
    }
}
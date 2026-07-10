using AwesomeAssertions;
using Sagittaras.Progression;
using Sagittaras.Progression.Formulas;

namespace Sagittaras.Tests.Progression.Formulas;

public class ExponentialScalingFormulaTest
{
    /// <summary>
    ///     Verify calculated value for level 1.
    /// </summary>
    [Fact]
    public void Calculate_Level1()
    {
        const int baseValue = 100;
        
        ExponentialScalingFormula formula = new(baseValue: baseValue);
        Experience result = formula.Calculate(1);
        result.Should().Be(baseValue);
    }

    /// <summary>
    ///     Verify calculated value for multiple levels in progression.
    /// </summary>
    [Fact]
    public void Calculate_Progression()
    {
        const float multiplier = 1.25f;
        const int baseValue = 200;

        // xp = base * multiplier ^ (level - 1)
        ExponentialScalingFormula formula = new(baseValue, multiplier);
        
        Experience level2 = formula.Calculate(2);
        level2.Should().Be(250); 

        Experience level3 = formula.Calculate(3);
        level3.Should().Be(312);

        Experience level5 = formula.Calculate(5);
        level5.Should().Be(488);
    }
}
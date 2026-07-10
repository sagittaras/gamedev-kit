using AwesomeAssertions;
using Sagittaras.Dices;
using Sagittaras.Dices.Probability;

namespace Sagittaras.Tests.Dices;

public class ChanceTableTest
{
    /// <summary>
    ///     Normalization summary cannot be converted to <see cref="Chance"/>.
    /// </summary>
    /// <remarks>
    ///     A total summary of 150 should be normalized to 100%.
    /// </remarks>
    [Fact]
    public void Normalization()
    {
        this.Invoking(_ =>
        {
            ChanceTable<string> table = new(new Dictionary<string, Chance>
            {
                { "a", 50f },
                { "b", 50f },
                { "c", 50f }
            });
        }).Should().NotThrow();
    }
}
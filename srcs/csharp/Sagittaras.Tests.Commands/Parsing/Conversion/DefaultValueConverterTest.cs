using System.Globalization;
using AwesomeAssertions;
using Sagittaras.Commands.Parsing.Conversion;
using Sagittaras.Tests.Commands.Fixtures;

namespace Sagittaras.Tests.Commands.Parsing.Conversion;

public class DefaultValueConverterTest
{
    private readonly DefaultValueConverter _converter = new();

    /// <summary>
    ///     Verify that a string is handed over as it was typed.
    /// </summary>
    [Fact]
    public void TryConvert_HandsAStringOver()
    {
        Convert<string>("ancient guardian").Should().Be("ancient guardian");
    }

    /// <summary>
    ///     Verify that the integer types are read, negative values included.
    /// </summary>
    [Theory]
    [InlineData("3", 3)]
    [InlineData("-3", -3)]
    [InlineData("0", 0)]
    public void TryConvert_ReadsIntegers(string text, int expected)
    {
        Convert<int>(text).Should().Be(expected);
    }

    /// <summary>
    ///     Verify that every integer type is read, and as that very type.
    /// </summary>
    /// <remarks>
    ///     <see cref="ushort"/> and <see cref="sbyte"/> went unread while the types were a chain of
    ///     conditions - a missing row is easy to see, a missing branch is not.
    /// </remarks>
    [Theory]
    [InlineData(typeof(byte))]
    [InlineData(typeof(sbyte))]
    [InlineData(typeof(short))]
    [InlineData(typeof(ushort))]
    [InlineData(typeof(int))]
    [InlineData(typeof(uint))]
    [InlineData(typeof(long))]
    [InlineData(typeof(ulong))]
    public void TryConvert_ReadsEveryIntegerType(Type valueType)
    {
        _converter.TryConvert("7", valueType, out object? value).Should().BeTrue();
        value.Should().BeOfType(valueType);
        System.Convert.ToInt64(value).Should().Be(7);
    }

    /// <summary>
    ///     Verify that a decimal point is read the same way under any culture of the running build.
    /// </summary>
    /// <remarks>
    ///     A console line is not a document. Under a comma-decimal culture a culture-sensitive read would
    ///     take <c>1.5</c> for fifteen, which is the kind of difference nobody would look for in a command.
    /// </remarks>
    [Fact]
    public void TryConvert_ReadsDecimalsInvariantly()
    {
        CultureInfo original = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("cs-CZ");

            Convert<float>("1.5").Should().Be(1.5f);
            Convert<double>("-0.25").Should().Be(-0.25);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    /// <summary>
    ///     Verify that a boolean is read the way a console line tends to spell one.
    /// </summary>
    [Theory]
    [InlineData("true", true)]
    [InlineData("TRUE", true)]
    [InlineData("yes", true)]
    [InlineData("on", true)]
    [InlineData("1", true)]
    [InlineData("false", false)]
    [InlineData("no", false)]
    [InlineData("off", false)]
    [InlineData("0", false)]
    public void TryConvert_ReadsBooleanSpellings(string text, bool expected)
    {
        Convert<bool>(text).Should().Be(expected);
    }

    /// <summary>
    ///     Verify that an enum is read by the name of its member, ignoring the case.
    /// </summary>
    [Theory]
    [InlineData("hostile", SpawnMode.Hostile)]
    [InlineData("Idle", SpawnMode.Idle)]
    [InlineData("IDLE", SpawnMode.Idle)]
    public void TryConvert_ReadsEnumMembers(string text, SpawnMode expected)
    {
        Convert<SpawnMode>(text).Should().Be(expected);
    }

    /// <summary>
    ///     Verify that a nullable type is read as the type it wraps.
    /// </summary>
    [Fact]
    public void TryConvert_UnwrapsNullability()
    {
        _converter.TryConvert("3", typeof(int?), out object? value).Should().BeTrue();
        value.Should().Be(3);
    }

    /// <summary>
    ///     Verify that an identifier and a single character are read as well.
    /// </summary>
    [Fact]
    public void TryConvert_ReadsIdentifiersAndCharacters()
    {
        Convert<Guid>("2f1a9d64-5b3c-4e7f-8a21-0c9d6e5b4a37").Should().NotBe(Guid.Empty);
        Convert<char>("x").Should().Be('x');
    }

    /// <summary>
    ///     Verify that a date is read in the forms the runtime reads under the invariant culture.
    /// </summary>
    [Theory]
    [InlineData("2026-09-21", 2026, 9, 21, 0, 0)]
    [InlineData("2026-09-21T14:30", 2026, 9, 21, 14, 30)]
    [InlineData("2026-09-21 14:30", 2026, 9, 21, 14, 30)]
    [InlineData("21 Sep 2026", 2026, 9, 21, 0, 0)]
    public void TryConvert_ReadsDates(string text, int year, int month, int day, int hour, int minute)
    {
        Convert<DateTime>(text).Should().Be(new DateTime(year, month, day, hour, minute, 0));
    }

    /// <summary>
    ///     Verify that a time alone is read as that time of today.
    /// </summary>
    [Fact]
    public void TryConvert_ReadsATimeAloneAsToday()
    {
        Convert<DateTime>("14:30").TimeOfDay.Should().Be(new TimeSpan(14, 30, 0));
    }

    /// <summary>
    ///     Verify that a numeric date is read month first, whatever keyboard it was typed on.
    /// </summary>
    /// <remarks>
    ///     The price of reading whatever the runtime reads under the invariant culture, pinned down here so
    ///     that it stays a decision rather than a surprise: a day of twelve or less swaps with the month
    ///     without a word, while a greater one is refused. A date read the same by everyone is written the
    ///     ISO way.
    /// </remarks>
    [Fact]
    public void TryConvert_ReadsANumericDateMonthFirst()
    {
        Convert<DateTime>("01.02.2026").Should().Be(new DateTime(2026, 1, 2));

        _converter.TryConvert("21.09.2026", typeof(DateTime), out object? value).Should().BeFalse();
        value.Should().BeNull();
    }

    /// <summary>
    ///     Verify that text which is not the declared type is answered with false instead of an exception.
    /// </summary>
    /// <remarks>
    ///     A value spelled wrong is an ordinary thing to happen to a console, and the parser turns the answer
    ///     into a reason it can point at the token with.
    /// </remarks>
    [Theory]
    [InlineData("many", typeof(int))]
    [InlineData("perhaps", typeof(bool))]
    [InlineData("furious", typeof(SpawnMode))]
    [InlineData("xy", typeof(char))]
    [InlineData("1.5", typeof(int))]
    [InlineData("300", typeof(byte))]
    [InlineData("-1", typeof(ushort))]
    [InlineData("tomorrow", typeof(DateTime))]
    [InlineData("2026-13-01", typeof(DateTime))]
    public void TryConvert_RefusesWhatItCannotRead(string text, Type valueType)
    {
        _converter.TryConvert(text, valueType, out object? value).Should().BeFalse();
        value.Should().BeNull();
    }

    /// <summary>
    ///     Verify that a separator a number is not written with is refused rather than read around.
    /// </summary>
    /// <remarks>
    ///     This is what the converter is not <c>Convert.ChangeType</c> for: it allows a thousands separator,
    ///     and under the invariant culture that makes a comma one - so <c>1,5</c> typed on a comma-decimal
    ///     keyboard would quietly become fifteen instead of pointing at the token.
    /// </remarks>
    [Theory]
    [InlineData("1,5", typeof(double))]
    [InlineData("1,5", typeof(float))]
    [InlineData("1,5", typeof(decimal))]
    [InlineData("1,000", typeof(int))]
    public void TryConvert_RefusesAThousandsSeparator(string text, Type valueType)
    {
        _converter.TryConvert(text, valueType, out object? value).Should().BeFalse();
        value.Should().BeNull();
    }

    /// <summary>
    ///     Verify that a type the converter knows nothing about is left to somebody else.
    /// </summary>
    /// <remarks>
    ///     Which is the whole point of the converter being a seam - the types of a game arrive through one of
    ///     its own.
    /// </remarks>
    [Fact]
    public void TryConvert_LeavesUnknownTypesAlone()
    {
        _converter.TryConvert("anything", typeof(SpawnEntityCommand), out object? value).Should().BeFalse();
        value.Should().BeNull();
    }

    /// <summary>
    ///     Reads the text as the given type, expecting it to be read.
    /// </summary>
    /// <param name="text">Text to read.</param>
    /// <typeparam name="T">Type to read the text as.</typeparam>
    /// <returns>Returns the value that was read.</returns>
    private T Convert<T>(string text)
    {
        _converter.TryConvert(text, typeof(T), out object? value).Should().BeTrue();

        return (T)value!;
    }
}

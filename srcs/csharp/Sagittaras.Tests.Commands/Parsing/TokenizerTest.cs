using AwesomeAssertions;
using Sagittaras.Commands.Parsing;
using Sagittaras.Commands.Parsing.Errors;
using Sagittaras.Commands.Parsing.Tokens;

namespace Sagittaras.Tests.Commands.Parsing;

public class TokenizerTest
{
    private readonly List<Token> _tokens = new();

    /// <summary>
    ///     Verify that the input is cut on white space, with every piece left for the later steps to name.
    /// </summary>
    [Fact]
    public void TryTokenize_CutsOnWhiteSpace()
    {
        Tokenize("spawn entity goblin");

        _tokens.Select(token => token.Lexeme).Should().Equal("spawn", "entity", "goblin");
        _tokens.Select(token => token.Type).Should().AllBeEquivalentTo(TokenType.Unknown);
    }

    /// <summary>
    ///     Verify that a token knows where it sits in the input, which is what a console underlines by.
    /// </summary>
    [Fact]
    public void TryTokenize_KeepsThePlaceInTheInput()
    {
        Tokenize("spawn  entity");

        _tokens[0].Start.Should().Be(0);
        _tokens[0].Length.Should().Be(5);
        _tokens[1].Start.Should().Be(7);
        _tokens[1].Length.Should().Be(6);
    }

    /// <summary>
    ///     Verify that an input of nothing but white space yields no tokens and no complaint.
    /// </summary>
    /// <remarks>
    ///     Cutting an empty line is not a failure of the cutting - it is the parser that decides there is
    ///     nothing to run.
    /// </remarks>
    [Fact]
    public void TryTokenize_LeavesBlankInputEmpty()
    {
        Tokenize("   ");

        _tokens.Should().BeEmpty();
    }

    /// <summary>
    ///     Verify that a quoted value survives the white space inside it.
    /// </summary>
    [Fact]
    public void TryTokenize_KeepsQuotedWhiteSpaceTogether()
    {
        Tokenize("spawn \"ancient guardian\"");

        _tokens.Select(token => token.Lexeme).Should().Equal("spawn", "ancient guardian");
    }

    /// <summary>
    ///     Verify that the quotes are left out of the lexeme but stay counted in the span.
    /// </summary>
    /// <remarks>
    ///     The two differ on purpose: the value is what was inside, while pointing back at the input has to
    ///     count the quotes that are written there.
    /// </remarks>
    [Fact]
    public void TryTokenize_CountsQuotesInTheSpanOnly()
    {
        Tokenize("\"ancient guardian\"");

        _tokens[0].Lexeme.Should().Be("ancient guardian").And.HaveLength(16);
        _tokens[0].Length.Should().Be(18);
    }

    /// <summary>
    ///     Verify that a quote nobody closes is reported, pointing at where it was opened.
    /// </summary>
    [Fact]
    public void TryTokenize_ReportsUnterminatedQuote()
    {
        bool cut = Tokenizer.TryTokenize("spawn \"ancient", _tokens, out IParseError? error);

        cut.Should().BeFalse();
        error.Should().NotBeNull();
        error!.Code.Should().Be(ParseErrorCode.UnterminatedQuote);
        error.Token.Should().NotBeNull();
        error.Token!.Value.Start.Should().Be(6);
    }

    /// <summary>
    ///     Verify that an equals sign pairs an option with its value in a single piece of input.
    /// </summary>
    [Fact]
    public void TryTokenize_SplitsTheEqualsForm()
    {
        Tokenize("--scene=arena");

        _tokens.Select(token => token.Lexeme).Should().Equal("--scene", "arena");
        _tokens.Select(token => token.Type).Should().Equal(TokenType.Option, TokenType.OptionValue);
        _tokens[0].Start.Should().Be(0);
        _tokens[0].Length.Should().Be(7);
        _tokens[1].Start.Should().Be(8);
        _tokens[1].Length.Should().Be(5);
    }

    /// <summary>
    ///     Verify that the value of the equals form may be quoted.
    /// </summary>
    [Fact]
    public void TryTokenize_ReadsQuotedValueOfTheEqualsForm()
    {
        Tokenize("--scene=\"the arena\"");

        _tokens.Select(token => token.Lexeme).Should().Equal("--scene", "the arena");
    }

    /// <summary>
    ///     Verify that only the first equals sign splits, so a value may carry one of its own.
    /// </summary>
    [Fact]
    public void TryTokenize_SplitsOnTheFirstEqualsOnly()
    {
        Tokenize("--filter=name=goblin");

        _tokens.Select(token => token.Lexeme).Should().Equal("--filter", "name=goblin");
    }

    /// <summary>
    ///     Verify that an equals sign in a plain value is left alone.
    /// </summary>
    /// <remarks>
    ///     The pairing belongs to an option, and a value that happens to look like an assignment is still
    ///     just a value.
    /// </remarks>
    [Fact]
    public void TryTokenize_LeavesEqualsInPlainValue()
    {
        Tokenize("name=goblin");

        _tokens.Should().HaveCount(1);
        _tokens[0].Lexeme.Should().Be("name=goblin");
    }

    /// <summary>
    ///     Verify that a negative number is a value and not an option.
    /// </summary>
    /// <remarks>
    ///     A dash is the calling sign of an option, which is why what stands behind it has to be looked at -
    ///     <c>-3</c> and <c>-.5</c> are values a command takes, not names it answers to.
    /// </remarks>
    [Theory]
    [InlineData("-3")]
    [InlineData("-.5")]
    public void LooksNamed_TellsNegativeNumbersFromOptions(string value)
    {
        Tokenizer.LooksNamed(value).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that both the long and the short calling sign name an option.
    /// </summary>
    [Theory]
    [InlineData("--scene")]
    [InlineData("-s")]
    public void LooksNamed_RecognizesTheCallingSign(string value)
    {
        Tokenizer.LooksNamed(value).Should().BeTrue();
    }

    /// <summary>
    ///     Verify that a lone dash is a value, as there is no name behind it.
    /// </summary>
    [Fact]
    public void LooksNamed_TakesALoneDashForAValue()
    {
        Tokenizer.LooksNamed("-").Should().BeFalse();
    }

    /// <summary>
    ///     Cuts the input, expecting it to be cut through.
    /// </summary>
    /// <param name="input">Input to cut.</param>
    private void Tokenize(string input)
    {
        Tokenizer.TryTokenize(input, _tokens, out IParseError? error).Should().BeTrue();
        error.Should().BeNull();
    }
}

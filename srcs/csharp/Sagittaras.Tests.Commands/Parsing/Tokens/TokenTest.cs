using AwesomeAssertions;
using Sagittaras.Commands.Parsing.Tokens;

namespace Sagittaras.Tests.Commands.Parsing.Tokens;

public class TokenTest
{
    private static readonly Token Goblin = new(TokenType.Argument, "goblin", 13, 6);

    /// <summary>
    ///     Verify that two tokens of the same role over the same piece of the input are equal.
    /// </summary>
    [Fact]
    public void Equals_HoldsForTheSameRoleOverTheSamePiece()
    {
        Token same = new(TokenType.Argument, "goblin", 13, 6);

        same.Equals(Goblin).Should().BeTrue();
        (same == Goblin).Should().BeTrue();
        (same != Goblin).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that the same text at another place is another token.
    /// </summary>
    /// <remarks>
    ///     <c>goblin goblin</c> holds two words to point at, and a console underlining the second must not
    ///     underline the first.
    /// </remarks>
    [Fact]
    public void Equals_TellsTheSameTextAtAnotherPlaceApart()
    {
        Token elsewhere = new(TokenType.Argument, "goblin", 20, 6);

        elsewhere.Should().NotBe(Goblin);
        (elsewhere == Goblin).Should().BeFalse();
    }

    /// <summary>
    ///     Verify that the same piece of the input playing another role is another token.
    /// </summary>
    [Fact]
    public void Equals_TellsAnotherRoleApart()
    {
        Token unmatched = new(TokenType.Unknown, "goblin", 13, 6);

        unmatched.Should().NotBe(Goblin);
    }

    /// <summary>
    ///     Verify that a quoted token is told apart from a bare one carrying the same text.
    /// </summary>
    /// <remarks>
    ///     The lexeme is the same, the span is not - the quotes are written in the input and take up room.
    /// </remarks>
    [Fact]
    public void Equals_TellsAQuotedTokenFromABareOne()
    {
        Token quoted = new(TokenType.Argument, "goblin", 13, 8);

        quoted.Should().NotBe(Goblin);
    }

    /// <summary>
    ///     Verify that equal tokens hash alike, so they can key a set or a dictionary.
    /// </summary>
    [Fact]
    public void GetHashCode_AgreesWithEquality()
    {
        Token same = new(TokenType.Argument, "goblin", 13, 6);

        same.GetHashCode().Should().Be(Goblin.GetHashCode());
        new HashSet<Token> { Goblin, same }.Should().HaveCount(1);
    }

    /// <summary>
    ///     Verify that a token compared through its boxed form still answers the same way.
    /// </summary>
    [Fact]
    public void Equals_AnswersTheSameWhenBoxed()
    {
        object boxed = new Token(TokenType.Argument, "goblin", 13, 6);

        Goblin.Equals(boxed).Should().BeTrue();
        Goblin.Equals("goblin").Should().BeFalse();
    }

    /// <summary>
    ///     Verify that a token reads plainly, which is what a failing assertion prints it as.
    /// </summary>
    [Fact]
    public void ToString_NamesTheRoleTheTextAndThePlace()
    {
        Goblin.ToString().Should().Be("Argument 'goblin' at 13");
    }
}

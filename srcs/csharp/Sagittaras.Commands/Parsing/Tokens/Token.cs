using System;

namespace Sagittaras.Commands.Parsing.Tokens
{
    /// <summary>
    ///     Represents a single piece of the input, together with the role the parser gave it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Cutting the input into tokens and deciding what they mean are two different things. The
    ///         lexeme and its place in the input are known the moment the input is cut; the
    ///         <see cref="Type"/> is only known once the command has been resolved and the values bound,
    ///         and it is the finished classification that <see cref="IParseResult.Tokens"/> hands out.
    ///     </para>
    ///     <para>
    ///         The token keeps its place in the original input, which is what lets a console point at the
    ///         piece it has something to say about - underlining the word a parsing error names, or
    ///         colouring a command differently from its values while the line is still being typed.
    ///     </para>
    ///     <para>
    ///         Two tokens are equal when they play the same role over the same piece of the same input -
    ///         the same text at a different place is a different token, as it is a different word to point
    ///         at.
    ///     </para>
    /// </remarks>
    public readonly struct Token : IEquatable<Token>
    {
        /// <summary>
        ///     Creates a token of the given role over a piece of the input.
        /// </summary>
        /// <param name="type">Role the token plays in the input.</param>
        /// <param name="lexeme">Text the token carries, with the quoting already resolved.</param>
        /// <param name="start">Index the token starts at in the original input.</param>
        /// <param name="length">Number of characters the token takes up in the original input.</param>
        public Token(TokenType type, string lexeme, int start, int length)
        {
            Type = type;
            Lexeme = lexeme;
            Start = start;
            Length = length;
        }

        /// <summary>
        ///     Role the token plays in the input.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        ///     Text the token carries.
        /// </summary>
        /// <remarks>
        ///     Quoting is already resolved, so a quoted token carries what was inside the quotes. An option
        ///     keeps the prefix it was written with - stripping it is a matter of matching the option, which
        ///     happens later and knows what it is matching against.
        /// </remarks>
        public string Lexeme { get; }

        /// <summary>
        ///     Index the token starts at in the original input.
        /// </summary>
        public int Start { get; }

        /// <summary>
        ///     Number of characters the token takes up in the original input.
        /// </summary>
        /// <remarks>
        ///     Told apart from the length of <see cref="Lexeme"/> on purpose: a quoted token takes up the
        ///     quotes as well, and pointing back at the input has to count them.
        /// </remarks>
        public int Length { get; }

        /// <summary>
        ///     Tells whether two tokens play the same role over the same piece of the input.
        /// </summary>
        /// <param name="left">Token on the left.</param>
        /// <param name="right">Token on the right.</param>
        /// <returns>Returns true when the tokens are equal; otherwise false.</returns>
        public static bool operator ==(Token left, Token right) => left.Equals(right);

        /// <summary>
        ///     Tells whether two tokens differ in their role or in the piece of the input they cover.
        /// </summary>
        /// <param name="left">Token on the left.</param>
        /// <param name="right">Token on the right.</param>
        /// <returns>Returns true when the tokens differ; otherwise false.</returns>
        public static bool operator !=(Token left, Token right) => !left.Equals(right);

        /// <inheritdoc />
        public bool Equals(Token other)
        {
            return Type == other.Type
                   && Start == other.Start
                   && Length == other.Length
                   && string.Equals(Lexeme, other.Lexeme, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Token other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Lexeme, Start, Length);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Type} '{Lexeme}' at {Start}";
        }
    }
}

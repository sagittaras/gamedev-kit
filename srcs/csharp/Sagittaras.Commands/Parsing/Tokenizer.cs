using System.Collections.Generic;
using System.Text;
using Sagittaras.Commands.Parsing.Errors;
using Sagittaras.Commands.Parsing.Tokens;

namespace Sagittaras.Commands.Parsing
{
    /// <summary>
    ///     Cuts a line of input into the pieces the rest of the parsing works over.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Knows nothing about commands. It splits on white space, resolves the quoting, and leaves
    ///         every piece as <see cref="TokenType.Unknown"/> - what a piece means is decided by the walk
    ///         through the hierarchy and the binding that follow.
    ///     </para>
    ///     <para>
    ///         The one thing it does decide is the pairing an equals sign spells out: <c>--scene=arena</c>
    ///         names an option and hands it a value in a single piece of input, and that pairing is lexical,
    ///         so it is read here rather than guessed at later.
    ///     </para>
    /// </remarks>
    internal static class Tokenizer
    {
        /// <summary>
        ///     Tries to cut the input into tokens.
        /// </summary>
        /// <param name="input">Input to cut.</param>
        /// <param name="tokens">Tokens the input was cut into, filled as far as the cutting got.</param>
        /// <param name="error">Reason the cutting failed, or null when it did not.</param>
        /// <returns>Returns true when the whole input was cut; otherwise false.</returns>
        internal static bool TryTokenize(string input, List<Token> tokens, out IParseError? error)
        {
            int index = 0;

            while (index < input.Length)
            {
                while (index < input.Length && char.IsWhiteSpace(input[index]))
                {
                    index++;
                }

                if (index >= input.Length)
                {
                    break;
                }

                if (!TryReadToken(input, ref index, tokens, out error))
                {
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        ///     Tells whether the piece of input starting at the index carries an option's calling sign.
        /// </summary>
        /// <remarks>
        ///     A dash followed by a digit or a decimal point is a negative number, not a name - which is the
        ///     whole reason this is asked about the character behind the dash at all.
        /// </remarks>
        /// <param name="value">Value to look at.</param>
        /// <param name="index">Index the piece starts at.</param>
        /// <returns>Returns true when the piece names an option; otherwise false.</returns>
        internal static bool LooksNamed(string value, int index = 0)
        {
            if (index + 1 >= value.Length || value[index] != '-')
            {
                return false;
            }

            char behind = value[index + 1];

            return !char.IsDigit(behind) && behind != '.';
        }

        /// <summary>
        ///     Reads a single token, resolving the quoting and the equals sign on the way.
        /// </summary>
        /// <param name="input">Input being cut.</param>
        /// <param name="index">Index to read from, left behind the token that was read.</param>
        /// <param name="tokens">Tokens to add the result to - two of them for the equals sign form.</param>
        /// <param name="error">Reason the reading failed, or null when it did not.</param>
        /// <returns>Returns true when a token was read; otherwise false.</returns>
        private static bool TryReadToken(string input, ref int index, List<Token> tokens, out IParseError? error)
        {
            int start = index;
            bool named = LooksNamed(input, index);
            StringBuilder lexeme = new();
            int equalsAt = -1;
            int equalsInLexeme = -1;

            while (index < input.Length && !char.IsWhiteSpace(input[index]))
            {
                if (input[index] == '"')
                {
                    if (!TryReadQuoted(input, ref index, start, lexeme, out error))
                    {
                        return false;
                    }

                    continue;
                }

                if (named && input[index] == '=' && equalsAt < 0)
                {
                    equalsAt = index;
                    equalsInLexeme = lexeme.Length;
                }

                lexeme.Append(input[index]);
                index++;
            }

            error = null;

            if (equalsAt < 0)
            {
                tokens.Add(new Token(TokenType.Unknown, lexeme.ToString(), start, index - start));
                return true;
            }

            string whole = lexeme.ToString();
            tokens.Add(new Token(TokenType.Option, whole.Substring(0, equalsInLexeme), start, equalsAt - start));
            tokens.Add(new Token(TokenType.OptionValue, whole.Substring(equalsInLexeme + 1), equalsAt + 1, index - equalsAt - 1));

            return true;
        }

        /// <summary>
        ///     Reads a quoted stretch into the token being read.
        /// </summary>
        /// <remarks>
        ///     The quotes themselves are left out of the lexeme but stay counted in the span of the token,
        ///     which is what keeps a console able to point back at the input.
        /// </remarks>
        /// <param name="input">Input being cut.</param>
        /// <param name="index">Index of the opening quote, left behind the closing one.</param>
        /// <param name="start">Index the whole token starts at, for the error to point at.</param>
        /// <param name="lexeme">Lexeme being built.</param>
        /// <param name="error">Reason the reading failed, or null when it did not.</param>
        /// <returns>Returns true when the stretch was closed; otherwise false.</returns>
        private static bool TryReadQuoted(string input, ref int index, int start, StringBuilder lexeme, out IParseError? error)
        {
            int opening = index;
            index++;

            int closing = input.IndexOf('"', index);
            if (closing < 0)
            {
                Token token = new(TokenType.Unknown, input.Substring(opening), start, input.Length - start);
                error = new ParseError(ParseErrorCode.UnterminatedQuote, "The input opens a quote that is never closed.", token);

                return false;
            }

            lexeme.Append(input, index, closing - index);
            index = closing + 1;
            error = null;

            return true;
        }
    }
}

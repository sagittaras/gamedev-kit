using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sagittaras.Commands.Parsing.Conversion
{
    /// <summary>
    ///     Reads the types a console line is usually spelled in.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Every type it knows is a single row of a table, each with a reader of its own. A type missing
    ///         from the table is visible as a missing row - which a chain of conditions never made it, and
    ///         which is how <see cref="ushort"/> and <see cref="sbyte"/> went unread for a while.
    ///     </para>
    ///     <para>
    ///         Numbers are read with the invariant culture and with the number styles spelled out, on
    ///         purpose. A command typed into a console is not a document: a build running under a
    ///         comma-decimal locale must not read <c>1.5</c> differently, and <c>1,5</c> must be refused
    ///         rather than taken for fifteen - which is what a thousands separator allowed by default would
    ///         make of it. That is also why the table is not <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>.
    ///     </para>
    ///     <para>
    ///         Dates are read by whatever the runtime reads under the invariant culture - an ISO date with or
    ///         without its time, a month spelled by name, a time alone meaning today. Numeric dates are read
    ///         month first, so <c>21.09.2026</c> is refused, but <c>01.02.2026</c> is the second of January;
    ///         a date that has to be read the same by everyone is best written the ISO way.
    ///     </para>
    ///     <para>
    ///         Enums are the one kind that cannot be a row, as there is one per enum type. They are read by the
    ///         names of their members, ignoring the case the way the rest of the input is matched - and never by
    ///         a number, which would let a value the enum does not declare slip through. Anything else is left
    ///         to a converter of the game's own.
    ///     </para>
    /// </remarks>
    public class DefaultValueConverter : IValueConverter
    {
        /// <summary>
        ///     Styles the integer types are read in - a sign and the surrounding white space, nothing more.
        /// </summary>
        private const NumberStyles Integers = NumberStyles.Integer;

        /// <summary>
        ///     Styles the decimal types are read in - a point and an exponent, but no thousands separator.
        /// </summary>
        private const NumberStyles Decimals = NumberStyles.Float;

        /// <summary>
        ///     Culture every number is read in, whatever the running build is set to.
        /// </summary>
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        /// <summary>
        ///     Readers of the types the converter knows, one per type.
        /// </summary>
        private static readonly Dictionary<Type, Reader> Readers = new()
        {
            [typeof(string)] = ReadString,
            [typeof(bool)] = ReadBoolean,
            [typeof(char)] = ReadCharacter,
            [typeof(Guid)] = Read((string text, out Guid value) => Guid.TryParse(text, out value)),
            [typeof(DateTime)] = Read((string text, out DateTime value) => DateTime.TryParse(text, Invariant, DateTimeStyles.None, out value)),
            [typeof(byte)] = Read((string text, out byte value) => byte.TryParse(text, Integers, Invariant, out value)),
            [typeof(sbyte)] = Read((string text, out sbyte value) => sbyte.TryParse(text, Integers, Invariant, out value)),
            [typeof(short)] = Read((string text, out short value) => short.TryParse(text, Integers, Invariant, out value)),
            [typeof(ushort)] = Read((string text, out ushort value) => ushort.TryParse(text, Integers, Invariant, out value)),
            [typeof(int)] = Read((string text, out int value) => int.TryParse(text, Integers, Invariant, out value)),
            [typeof(uint)] = Read((string text, out uint value) => uint.TryParse(text, Integers, Invariant, out value)),
            [typeof(long)] = Read((string text, out long value) => long.TryParse(text, Integers, Invariant, out value)),
            [typeof(ulong)] = Read((string text, out ulong value) => ulong.TryParse(text, Integers, Invariant, out value)),
            [typeof(float)] = Read((string text, out float value) => float.TryParse(text, Decimals, Invariant, out value)),
            [typeof(double)] = Read((string text, out double value) => double.TryParse(text, Decimals, Invariant, out value)),
            [typeof(decimal)] = Read((string text, out decimal value) => decimal.TryParse(text, Decimals, Invariant, out value)),
        };

        /// <summary>
        ///     Reads the text as the value of a single type.
        /// </summary>
        /// <param name="text">Text to read.</param>
        /// <param name="value">Value that was read, or null when the text could not be read as one.</param>
        /// <returns>Returns true when the text was read; otherwise false.</returns>
        private delegate bool Reader(string text, out object? value);

        /// <summary>
        ///     The shape every TryParse of the runtime has, for <see cref="Read{T}"/> to turn into a reader.
        /// </summary>
        /// <param name="text">Text to read.</param>
        /// <param name="value">Value that was read.</param>
        /// <typeparam name="T">Type the text is read as.</typeparam>
        /// <returns>Returns true when the text was read; otherwise false.</returns>
        private delegate bool TryParse<T>(string text, out T value);

        /// <inheritdoc />
        public bool TryConvert(string text, Type valueType, out object? value)
        {
            Type target = Nullable.GetUnderlyingType(valueType) ?? valueType;

            if (target.IsEnum)
            {
                return TryConvertEnum(text, target, out value);
            }

            if (Readers.TryGetValue(target, out Reader? reader))
            {
                return reader(text, out value);
            }

            value = null;
            return false;
        }

        /// <summary>
        ///     Turns a TryParse of the runtime into a reader of the table.
        /// </summary>
        /// <param name="parse">TryParse reading the type.</param>
        /// <typeparam name="T">Type the text is read as.</typeparam>
        /// <returns>Returns the reader handing the value over as an object.</returns>
        private static Reader Read<T>(TryParse<T> parse)
        {
            return (string text, out object? value) =>
            {
                bool read = parse(text, out T parsed);
                value = read ? parsed : null;

                return read;
            };
        }

        /// <summary>
        ///     Hands a string over as it was typed.
        /// </summary>
        /// <param name="text">Text to read.</param>
        /// <param name="value">The text itself.</param>
        /// <returns>Always returns true - any text is a string.</returns>
        private static bool ReadString(string text, out object? value)
        {
            value = text;

            return true;
        }

        /// <summary>
        ///     Reads the text as a boolean, accepting the way a console line tends to spell one.
        /// </summary>
        /// <remarks>
        ///     Only ever met on an option that is not a flag - a flag carries no text to read at all.
        /// </remarks>
        /// <param name="text">Text to read.</param>
        /// <param name="value">Value that was read, or null when the text is not a boolean.</param>
        /// <returns>Returns true when the text was read as a boolean; otherwise false.</returns>
        private static bool ReadBoolean(string text, out object? value)
        {
            bool? parsed = text.ToLowerInvariant() switch
            {
                "true" or "yes" or "on" or "1" => true,
                "false" or "no" or "off" or "0" => false,
                _ => null,
            };

            value = parsed;
            return parsed.HasValue;
        }

        /// <summary>
        ///     Reads the text as a single character.
        /// </summary>
        /// <param name="text">Text to read.</param>
        /// <param name="value">The character, or null when the text is not exactly one.</param>
        /// <returns>Returns true when the text is a single character; otherwise false.</returns>
        private static bool ReadCharacter(string text, out object? value)
        {
            value = text.Length == 1 ? text[0] : null;

            return value != null;
        }

        /// <summary>
        ///     Reads the text as a member of the enum, by its name and ignoring the case.
        /// </summary>
        /// <param name="text">Text to read.</param>
        /// <param name="target">Enum the value is read as.</param>
        /// <param name="value">Value that was read, or null when the text names no member.</param>
        /// <returns>Returns true when the text names a member of the enum; otherwise false.</returns>
        private static bool TryConvertEnum(string text, Type target, out object? value)
        {
            string? member = Enum.GetNames(target).FirstOrDefault(name => string.Equals(name, text, StringComparison.OrdinalIgnoreCase));

            value = member == null ? null : Enum.Parse(target, member);
            return member != null;
        }
    }
}

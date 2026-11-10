using System;

namespace Sagittaras.Commands.Parsing.Conversion
{
    /// <summary>
    ///     Turns a piece of the input into the value a parameter is declared to hold.
    /// </summary>
    /// <remarks>
    ///     The seam between the input being text and a command reading values. Whatever a game keeps its
    ///     own types of - an entity identifier, a vector, a currency - becomes reachable from a console line
    ///     by handing a converter of its own to <see cref="CommandParser"/>; the parser asks nothing else
    ///     about the types it binds.
    /// </remarks>
    public interface IValueConverter
    {
        /// <summary>
        ///     Tries to read the text as a value of the given type.
        /// </summary>
        /// <remarks>
        ///     Answers with false rather than throwing - a value the input spelled wrong is an ordinary
        ///     thing to happen, and the parser turns it into a reason it can point at the token with.
        /// </remarks>
        /// <param name="text">Text to read, as the input spelled it.</param>
        /// <param name="valueType">Type the value is read as, as declared by the parameter.</param>
        /// <param name="value">Value that was read, or null when the text could not be read as one.</param>
        /// <returns>Returns true when the text was read as the type; otherwise false.</returns>
        bool TryConvert(string text, Type valueType, out object? value);
    }
}

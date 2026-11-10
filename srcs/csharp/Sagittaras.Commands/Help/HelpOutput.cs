using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Sagittaras.Commands.Invocation;
using Sagittaras.Commands.Parameters;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Commands.Help
{
    /// <summary>
    ///     What the built-in help answers with - either the commands there are, or everything about one of them.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Carries the commands themselves, so a console with an interface of its own can lay the help out
    ///         its own way - a table of the arguments, the options coloured by kind, the subcommands as links -
    ///         and one without it prints <see cref="CommandOutput.Content"/>, which says the same in plain text.
    ///     </para>
    ///     <para>
    ///         A command does not know the branch it hangs under, so the help carries the whole
    ///         <see cref="Path"/> down to it. That is what lets the usage read <c>spawn entity &lt;entity&gt;</c>
    ///         rather than <c>entity &lt;entity&gt;</c>, which could not be typed.
    ///     </para>
    /// </remarks>
    public class HelpOutput : CommandOutput
    {
        /// <summary>
        ///     Creates a help of the given parts.
        /// </summary>
        /// <param name="path">Commands leading to the one described, empty for the overview.</param>
        /// <param name="commands">Commands the help lists.</param>
        /// <param name="content">Plain reading of the help.</param>
        private HelpOutput(IReadOnlyList<ICommand> path, IReadOnlyList<ICommand> commands, string content)
            : base(0, content)
        {
            Path = path;
            Commands = commands;
        }

        /// <summary>
        ///     Commands leading from the top of the hierarchy down to the one described.
        /// </summary>
        /// <remarks>
        ///     Empty for the overview of all the commands.
        /// </remarks>
        public IReadOnlyList<ICommand> Path { get; }

        /// <summary>
        ///     Command the help describes, or null for the overview.
        /// </summary>
        public ICommand? Command => Path.Count == 0 ? null : Path[^1];

        /// <summary>
        ///     Commands the help lists - all of them for the overview, the subcommands of a branch otherwise.
        /// </summary>
        public IReadOnlyList<ICommand> Commands { get; }

        /// <summary>
        ///     Creates the overview of the commands there are.
        /// </summary>
        /// <param name="commands">Commands reachable by naming them first.</param>
        /// <returns>Returns the overview.</returns>
        public static HelpOutput Overview(IReadOnlyList<ICommand> commands)
        {
            StringBuilder content = new("Available commands:");
            AppendRows(content, commands.Select(command => (command.Name, command.Description)).ToList());

            return new HelpOutput(Array.Empty<ICommand>(), commands, content.ToString());
        }

        /// <summary>
        ///     Creates the help about a single command.
        /// </summary>
        /// <param name="path">Commands leading from the top of the hierarchy down to the one described, itself last.</param>
        /// <returns>Returns the help about the last command of the path.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the path is empty.</exception>
        public static HelpOutput About(IReadOnlyList<ICommand> path)
        {
            Guard.Against.Zero(path.Count, "A help about a command needs the path down to it - the overview is what describes no command at all.");

            ICommand command = path[^1];

            return new HelpOutput(path, command.Subcommands, Describe(path, command));
        }

        /// <summary>
        ///     Writes out everything there is to know about the command.
        /// </summary>
        /// <param name="path">Commands leading down to the described one.</param>
        /// <param name="command">Command to describe.</param>
        /// <returns>Returns the plain reading of the help.</returns>
        private static string Describe(IReadOnlyList<ICommand> path, ICommand command)
        {
            string spelled = string.Join(" ", path.Select(step => step.Name));
            StringBuilder content = new($"{spelled} - {command.Description}");

            content.AppendLine().AppendLine().Append("Usage: ").Append(Usage(spelled, command));

            if (command.Subcommands.Count > 0)
            {
                AppendSection(content, "Subcommands:", command.Subcommands.Select(subcommand => (subcommand.Name, subcommand.Description)));
            }

            if (command.Arguments.Count > 0)
            {
                AppendSection(content, "Arguments:", command.Arguments.Select(argument => (argument.Name, Explain(argument))));
            }

            if (command.Options.Count > 0)
            {
                AppendSection(content, "Options:", command.Options.Select(option => (Label(option), Explain(option))));
            }

            return content.ToString();
        }

        /// <summary>
        ///     Spells the way the command is typed, with a placeholder for everything it takes.
        /// </summary>
        /// <remarks>
        ///     A required argument reads <c>&lt;name&gt;</c> and an optional one <c>[name]</c>; the options stand
        ///     behind the arguments, where the parser expects the path to have ended.
        /// </remarks>
        /// <param name="spelled">Path of the command, spelled.</param>
        /// <param name="command">Command to spell the usage of.</param>
        /// <returns>Returns the usage line.</returns>
        private static string Usage(string spelled, ICommand command)
        {
            StringBuilder usage = new(spelled);

            if (command.Subcommands.Count > 0)
            {
                usage.Append(" <subcommand>");
            }

            foreach (IArgument argument in command.Arguments)
            {
                usage.Append(argument.IsOptional ? $" [{argument.Name}]" : $" <{argument.Name}>");
            }

            if (command.Options.Count > 0)
            {
                usage.Append(" [options]");
            }

            return usage.ToString();
        }

        /// <summary>
        ///     Names an option by every calling sign it answers to, the way the input spells them.
        /// </summary>
        /// <param name="option">Option to name.</param>
        /// <returns>Returns the calling signs, and a placeholder for the value when the option takes one.</returns>
        private static string Label(IOption option)
        {
            string signs = string.Join(", ", new[] { option.Name }.Concat(option.Aliases).Select(Prefixed));

            return option.IsFlag ? signs : $"{signs} <value>";
        }

        /// <summary>
        ///     Spells a name with the prefix it is usually typed with - one dash for a single letter, two otherwise.
        /// </summary>
        /// <param name="name">Name of the option.</param>
        /// <returns>Returns the name with its prefix.</returns>
        private static string Prefixed(string name)
        {
            return name.Length == 1 ? $"-{name}" : $"--{name}";
        }

        /// <summary>
        ///     Explains an argument, with the value it falls back to when it may be left out.
        /// </summary>
        /// <param name="argument">Argument to explain.</param>
        /// <returns>Returns the explanation.</returns>
        private static string Explain(IArgument argument)
        {
            return argument.IsOptional ? WithDefault(argument.Description, argument.DefaultValue) : argument.Description;
        }

        /// <summary>
        ///     Explains an option, with the value it falls back to unless it is a flag.
        /// </summary>
        /// <remarks>
        ///     A flag is off until named, which says nothing worth repeating.
        /// </remarks>
        /// <param name="option">Option to explain.</param>
        /// <returns>Returns the explanation.</returns>
        private static string Explain(IOption option)
        {
            return option.IsFlag ? option.Description : WithDefault(option.Description, option.DefaultValue);
        }

        /// <summary>
        ///     Adds the default to a description, spelled the way the input would spell it.
        /// </summary>
        /// <remarks>
        ///     Spelled with the invariant culture, as the input is read with it - a default shown as <c>1,5</c>
        ///     would be refused if typed back.
        /// </remarks>
        /// <param name="description">Description of the parameter.</param>
        /// <param name="value">Default of the parameter.</param>
        /// <returns>Returns the description, with the default behind it when there is one.</returns>
        private static string WithDefault(string description, object? value)
        {
            return value == null ? description : $"{description} (default: {Convert.ToString(value, CultureInfo.InvariantCulture)})";
        }

        /// <summary>
        ///     Adds a titled section of rows, separated from what comes before by an empty line.
        /// </summary>
        /// <param name="content">Help being written.</param>
        /// <param name="title">Title of the section.</param>
        /// <param name="rows">Rows of the section, a label and its explanation each.</param>
        private static void AppendSection(StringBuilder content, string title, IEnumerable<(string Label, string Text)> rows)
        {
            content.AppendLine().AppendLine().Append(title);
            AppendRows(content, rows.ToList());
        }

        /// <summary>
        ///     Adds rows with the explanations lined up behind the longest label.
        /// </summary>
        /// <param name="content">Help being written.</param>
        /// <param name="rows">Rows to add, a label and its explanation each.</param>
        private static void AppendRows(StringBuilder content, IReadOnlyList<(string Label, string Text)> rows)
        {
            int width = rows.Select(row => row.Label.Length).DefaultIfEmpty().Max();

            foreach ((string label, string text) in rows)
            {
                content.AppendLine().Append("  ").Append(label.PadRight(width)).Append("  ").Append(text);
            }
        }
    }
}

using System.Linq;
using System.Text;

namespace Sagittaras.Commands.Invocation
{
    /// <summary>
    ///     The answer of a branch of the hierarchy named without one of its subcommands - what it offers.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Carries the branch itself rather than only a text about it, so a console with an interface of its
    ///         own can lay the subcommands out as a help of its own making - and one without it still prints
    ///         <see cref="CommandOutput.Content"/>, which lists them lined up.
    ///     </para>
    ///     <para>
    ///         It is also the pattern for an output of the game's own: derive from <see cref="CommandOutput"/>,
    ///         carry the payload, and keep the content a plain reading of the same answer. How it is shown is
    ///         left to whoever receives it.
    ///     </para>
    /// </remarks>
    public class UsageOutput : CommandOutput
    {
        /// <summary>
        ///     Exit code of a branch named alone.
        /// </summary>
        /// <remarks>
        ///     Not a success - the line stopped short of anything to run - but not a mistake of the input either,
        ///     which is why it is told apart from <c>RefusalOutput.UsageExitCode</c>.
        /// </remarks>
        public const int BranchExitCode = 1;

        /// <summary>
        ///     Creates the usage of the given branch.
        /// </summary>
        /// <param name="command">Branch whose subcommands are offered.</param>
        public UsageOutput(ICommand command)
            : base(BranchExitCode, Describe(command))
        {
            Command = command;
        }

        /// <summary>
        ///     Branch whose subcommands are offered.
        /// </summary>
        public ICommand Command { get; }

        /// <summary>
        ///     Writes out the subcommands the branch holds, with the descriptions lined up.
        /// </summary>
        /// <param name="command">Branch to describe.</param>
        /// <returns>Returns the plain reading of what the branch offers.</returns>
        private static string Describe(ICommand command)
        {
            StringBuilder builder = new($"Command '{command.Name}' has to be followed by one of:");
            int width = command.Subcommands.Select(subcommand => subcommand.Name.Length).DefaultIfEmpty().Max();

            foreach (ICommand subcommand in command.Subcommands)
            {
                builder.AppendLine();
                builder.Append("  ");
                builder.Append(subcommand.Name.PadRight(width));
                builder.Append("  ");
                builder.Append(subcommand.Description);
            }

            return builder.ToString();
        }
    }
}

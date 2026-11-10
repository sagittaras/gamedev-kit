using System.Collections.Generic;
using System.Linq;
using Sagittaras.Commands.Invocation;

namespace Sagittaras.Commands.Help
{
    /// <summary>
    ///     A mirror of a registered command within the help, answering with everything about the command it
    ///     mirrors.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         It bears the name and the description of the mirrored command, so the parser walks
    ///         <c>help spawn entity</c> the same way it walks <c>spawn entity</c>, and it mirrors the subcommands
    ///         the same way. It declares no parameters of its own - what follows the path of a help is nothing
    ///         to read - and it answers about the command it mirrors, never about itself, so a console receives
    ///         the real command with its real parameters.
    ///     </para>
    ///     <para>
    ///         The mirror of the help itself mirrors nothing further; the help holds mirrors, and a mirror of a
    ///         mirror would describe a command with none of the parameters of the one it stands for.
    ///     </para>
    /// </remarks>
    internal sealed class HelpTopic : CommandGroup
    {
        /// <summary>
        ///     Commands leading down to the mirrored one, itself last.
        /// </summary>
        private readonly IReadOnlyList<ICommand> _path;

        /// <summary>
        ///     Creates a mirror of the last command of the path.
        /// </summary>
        /// <param name="path">Commands leading down to the mirrored one, itself last.</param>
        /// <param name="mirrorSubcommands">Whether the subcommands of the mirrored command are mirrored too.</param>
        internal HelpTopic(IReadOnlyList<ICommand> path, bool mirrorSubcommands)
            : base(path[^1].Name, path[^1].Description)
        {
            _path = path;

            if (!mirrorSubcommands)
            {
                return;
            }

            foreach (ICommand subcommand in path[^1].Subcommands)
            {
                Add(new HelpTopic(path.Append(subcommand).ToArray(), true));
            }
        }

        /// <summary>
        ///     Answers with everything about the mirrored command.
        /// </summary>
        /// <param name="input">Values bound for this invocation.</param>
        /// <returns>Returns the help about the mirrored command.</returns>
        public override ICommandOutput Invoke(ICommandInput input)
        {
            return HelpOutput.About(_path);
        }
    }
}

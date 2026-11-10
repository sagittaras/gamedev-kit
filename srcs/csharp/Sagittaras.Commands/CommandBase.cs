using System;
using System.Collections.Generic;
using Sagittaras.Commands.Parameters;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Commands
{
    /// <summary>
    ///     Holds what every command has - a name, a description and the parameters it declares.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Not implemented directly. A command is written as a <see cref="Command"/> or an
    ///         <see cref="AsyncCommand"/> when it does the work itself, or as a <see cref="CommandGroup"/>
    ///         when it only holds others.
    ///     </para>
    ///     <para>
    ///         Everything is declared in the constructor and nothing changes afterwards. Registration is
    ///         where the cost belongs, and a command reached from a console line is then only read.
    ///     </para>
    /// </remarks>
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        ///     Positional arguments, in the order the input is expected to carry them.
        /// </summary>
        private readonly List<IArgument> _arguments = new();

        /// <summary>
        ///     Named options, in the order they were declared.
        /// </summary>
        private readonly List<IOption> _options = new();

        /// <summary>
        ///     Creates a command answering to the given name.
        /// </summary>
        /// <param name="name">Name of the command, serving as its calling sign.</param>
        /// <param name="description">Short description of the command's usage.</param>
        /// <exception cref="ArgumentException">Thrown when the name is empty or holds white space.</exception>
        protected CommandBase(string name, string description)
        {
            Guard.Against.NullOrWhiteSpace(name, "Command must be given a name to be called by.");
            Guard.Against.WhiteSpaceInside(name, $"Command name '{name}' must be a single token - the input is matched token by token, so a name holding white space could never be met as a whole.");

            Name = name;
            Description = description;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        /// <remarks>
        ///     Empty unless the command is a <see cref="CommandGroup"/>, which is the only shape a branch of
        ///     the hierarchy is written in.
        /// </remarks>
        public virtual IReadOnlyList<ICommand> Subcommands => Array.Empty<ICommand>();

        /// <inheritdoc />
        public IReadOnlyList<IArgument> Arguments => _arguments;

        /// <inheritdoc />
        public IReadOnlyList<IOption> Options => _options;

        /// <summary>
        ///     Declares the parameters the command reads its values from.
        /// </summary>
        /// <remarks>
        ///     Takes both kinds at once, as the difference between them is a matter of how the input is read
        ///     and not of what the command does with the value. Arguments keep the order they are declared
        ///     in, because that is the order the input is expected in; options are order-free.
        /// </remarks>
        /// <param name="parameters">Parameters to declare.</param>
        /// <exception cref="ArgumentException">Thrown when a parameter is neither an argument nor an option.</exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a positional argument is declared by a command that already holds subcommands - a
        ///     token behind it would be both a subcommand name and the value of the argument.
        /// </exception>
        protected void Accepts(params IParameter[] parameters)
        {
            foreach (IParameter parameter in parameters)
            {
                switch (parameter)
                {
                    case IOption option:
                        _options.Add(option);
                        break;
                    case IArgument argument:
                        Guard.Against.True(Subcommands.Count > 0, $"Command '{Name}' holds subcommands, so it cannot declare the positional argument '{argument.Name}' - a token behind it would be both a subcommand name and the value of the argument.");
                        _arguments.Add(argument);
                        break;
                    default:
                        throw new ArgumentException($"Parameter '{parameter.Name}' is neither an argument nor an option, so there is no way to read it from the input.", nameof(parameters));
                }
            }
        }
    }
}

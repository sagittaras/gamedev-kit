namespace Sagittaras.Commands.Parameters
{
    /// <summary>
    ///     Describes a value the command reads from its place in the input.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An argument has no calling sign - it is recognized by the position it holds among the
    ///         remaining tokens, so the order in which <see cref="ICommand.Arguments"/> lists them is the
    ///         order the input is expected in.
    ///     </para>
    ///     <para>
    ///         That is also why a command declaring arguments may not declare
    ///         <see cref="ICommand.Subcommands"/>: a bare token would be both a subcommand name and the
    ///         first argument, and no rule resolving that is worth the surprise it causes.
    ///     </para>
    /// </remarks>
    public interface IArgument : IParameter
    {
        /// <summary>
        ///     Indicates whether the input may end before reaching this argument.
        /// </summary>
        /// <remarks>
        ///     Optional arguments only ever come last - once one is allowed to be missing, nothing
        ///     positional behind it could be told apart from it.
        /// </remarks>
        bool IsOptional { get; }
    }
}

namespace Sagittaras.GuardClauses
{
    /// <summary>
    ///     Entrypoint for using Guard Clauses defined as extension methods.
    /// </summary>
    public class Guard : IGuardClause
    {
        private Guard()
        {
        }

        /// <summary>
        ///     An entry point for using guard clauses.
        /// </summary>
        public static IGuardClause Against { get; } = new Guard();
    }
}
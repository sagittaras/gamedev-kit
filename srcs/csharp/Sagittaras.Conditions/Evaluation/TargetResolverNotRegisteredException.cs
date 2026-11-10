using System;

namespace Sagittaras.Conditions.Evaluation
{
    /// <summary>
    ///     Represents an exception that occurs when a condition is evaluated without a target resolver registered for its target.
    /// </summary>
    /// <remarks>
    ///     This exception indicates an error in the condition data or evaluator setup, not in the game state.
    /// </remarks>
    public class TargetResolverNotRegisteredException : Exception
    {
        /// <summary>
        ///     Represents an exception that occurs when a condition is evaluated without a target resolver registered for its target.
        /// </summary>
        /// <param name="target">Condition target without a registered target resolver.</param>
        public TargetResolverNotRegisteredException(ConditionTarget target) : base($"No target resolver is registered for condition target `{target}`.")
        {
            Target = target;
        }

        /// <summary>
        ///     Condition target without a registered target resolver.
        /// </summary>
        public ConditionTarget Target { get; }
    }
}

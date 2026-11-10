using System;

namespace Sagittaras.Conditions.Evaluation
{
    /// <summary>
    ///     Represents an exception that occurs when a condition is evaluated without a condition handler registered for its type.
    /// </summary>
    /// <remarks>
    ///     This exception indicates an error in the condition data or evaluator setup, not in the game state.
    /// </remarks>
    public class ConditionHandlerNotRegisteredException : Exception
    {
        /// <summary>
        ///     Represents an exception that occurs when a condition is evaluated without a condition handler registered for its type.
        /// </summary>
        /// <param name="conditionType">Condition type without a registered condition handler.</param>
        public ConditionHandlerNotRegisteredException(ConditionType conditionType) : base($"No condition handler is registered for condition type `{conditionType}`.")
        {
            ConditionType = conditionType;
        }

        /// <summary>
        ///     Condition type without a registered condition handler.
        /// </summary>
        public ConditionType ConditionType { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sagittaras.Conditions.Evaluation
{
    /// <summary>
    ///     Represents an exception that occurs when conditions are evaluated in a context without a registered evaluator.
    /// </summary>
    /// <remarks>
    ///     This exception indicates an error in the manager setup or in the context type used at the call site,
    ///     not in the game state.
    /// </remarks>
    public class ConditionEvaluatorNotRegisteredException : Exception
    {
        /// <summary>
        ///     Represents an exception that occurs when conditions are evaluated in a context without a registered evaluator.
        /// </summary>
        /// <param name="contextType">Context type without a registered evaluator.</param>
        /// <param name="registeredContextTypes">Context types having a registered evaluator.</param>
        public ConditionEvaluatorNotRegisteredException(Type contextType, IEnumerable<Type> registeredContextTypes) : base(GetExceptionMessage(contextType, registeredContextTypes))
        {
            ContextType = contextType;
        }

        /// <summary>
        ///     Context type without a registered evaluator.
        /// </summary>
        public Type ContextType { get; }

        /// <summary>
        ///     Constructs the message listing the context types the manager actually knows.
        /// </summary>
        /// <remarks>
        ///     Evaluator is looked up by the context type taken from the call site, so the listing tells
        ///     whether the evaluator is missing at all, or just registered for a different type.
        /// </remarks>
        /// <param name="contextType">Context type without a registered evaluator.</param>
        /// <param name="registeredContextTypes">Context types having a registered evaluator.</param>
        /// <returns>A string describing the missing evaluator and the registered context types.</returns>
        private static string GetExceptionMessage(Type contextType, IEnumerable<Type> registeredContextTypes)
        {
            List<string> registered = registeredContextTypes.Select(type => $"`{type}`").ToList();
            string known = registered.Count > 0
                ? $"Registered context types: {string.Join(", ", registered)}."
                : "No context types are registered.";

            return $"No condition evaluator is registered for context type `{contextType}`. {known}";
        }
    }
}

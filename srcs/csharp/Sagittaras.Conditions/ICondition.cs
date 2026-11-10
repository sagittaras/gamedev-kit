using System.Collections.Generic;

namespace Sagittaras.Conditions
{
    /// <summary>
    ///     Condition that can be attached to an entity and used to validate whether the entity meets certain criteria.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        ///     Type of condition representing evaluation behavior and required parameters. 
        /// </summary>
        ConditionType ConditionType { get; }
        
        /// <summary>
        ///     Specified the target to which the condition is applied.
        /// </summary>
        ConditionTarget Target { get; }
        
        /// <summary>
        ///     Parameters required for the condition evaluation.;
        /// </summary>
        IReadOnlyList<int> Parameters { get; }

        /// <summary>
        ///     Indicates whether the result of condition evaluation should be negated.
        /// </summary>
        bool Negated { get; }
        
        /// <summary>
        ///     Comparison operator used for evaluation of parameter value and condition's target.
        /// </summary>
        /// <remarks>
        ///     Comparison applies only for cases where a single parameter is used for evaluation.
        /// </remarks>
        Comparison Comparison { get; }
    }
}
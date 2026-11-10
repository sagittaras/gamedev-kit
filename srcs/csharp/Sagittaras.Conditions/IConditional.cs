using System.Collections.Generic;

namespace Sagittaras.Conditions
{
    /// <summary>
    ///     Interface marking the entity as a source of conditions.
    /// </summary>
    public interface IConditional
    {
        /// <summary>
        ///     Conditions that the entity must meet.
        /// </summary>
        IReadOnlyList<ICondition> Conditions { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sagittaras.Dices.Extensions;
using Sagittaras.Dices.Probability;

namespace Sagittaras.Dices
{
    /// <summary>
    ///     Table of weighted probabilities for selecting entries based on their <see cref="Chance"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entries in the table.</typeparam>
    public readonly struct ChanceTable<T>
    {
        /// <summary>
        ///     Dice bag used for generation of random chances.
        /// </summary>
        private readonly IDiceBag _diceBag;

        /// <summary>
        ///     Represents a table of weighted probabilities for selecting entries based on their <see cref="Chance"/>.
        /// </summary>
        private readonly Chance[] _probabilities;

        /// <summary>
        ///     Maintains a collection of entries corresponding to a chance table, where each entry
        ///     represents an item that can be selected based on its associated probability.
        /// </summary>
        private readonly List<T> _entries;
        
        public ChanceTable(ICollection<KeyValuePair<T, Chance>> pairs, IDiceBag? diceBag = null)
        {
            _diceBag = diceBag ?? DiceBag.Instance;
            _entries = new List<T>();
            
            int probabilitySum = pairs.Aggregate(0, (sum, pair) => sum + pair.Value);
            int noChance = probabilitySum < Chance.MaxValue
                ? Chance.MaxValue - probabilitySum
                : 0;

            List<Chance> probabilities = new();
            int accumulation = 0;
            foreach ((T entry, Chance chance) in pairs)
            {
                accumulation += chance.Normalize(probabilitySum);

                probabilities.Add(accumulation);
                _entries.Add(entry);
            }

            if (noChance > 0)
            {
                probabilities.Add(noChance);
            }
            
            _probabilities = probabilities.ToArray();
        }
        
        /// <summary>
        ///     Attempts to randomly select an entry from the chance table based on weighted probabilities.
        /// </summary>
        /// <param name="entry">
        ///     The output parameter that receives the selected entry if the operation is successful;
        ///     otherwise, it is set to the default value of the specified type.
        /// </param>
        /// <returns>
        ///     Returns <c>true</c> if the selection was successful; otherwise, <c>false</c>.
        /// </returns>
        public bool TryNext([MaybeNullWhen(false)] out T entry)
        {
            entry = default;

            int index = Array.BinarySearch(_probabilities, _diceBag.NextChance());
            if (index < 0)
            {
                index = ~index;
            }

            try
            {
                entry = _entries[index];
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }
    }
}
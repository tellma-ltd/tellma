using System.Collections.Generic;
using System.Linq;

namespace Tellma.Utilities.Common
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Clones the dictionary into a new one such that modifications to
        /// the original dictionary do not affect the new dictionary.
        /// </summary>
        public static IDictionary<K, V> Clone<K, V>(this IDictionary<K, V> dic)
        {
            if (dic is null)
            {
                return null;
            }

            return dic.ToDictionary(e => e.Key, e => e.Value);
        }
    }
}

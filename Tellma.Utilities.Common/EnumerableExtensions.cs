using System.Collections.Generic;
using System.Linq;

namespace Tellma.Utilities.Common
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns every item in the <paramref name="collection"/> paired with its index.
        /// </summary>
        public static IEnumerable<(T, int)> Indexed<T>(this IEnumerable<T> collection)
        {
            foreach (var pair in collection.Select((e, i) => (e, i)))
            {
                yield return pair;
            }
        }
    }
}

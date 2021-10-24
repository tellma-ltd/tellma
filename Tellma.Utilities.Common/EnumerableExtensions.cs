using System.Collections.Generic;
using System.Linq;

namespace Tellma.Utilities.Common
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns every item in the <paramref name="source"/> paired with its index.
        /// </summary>
        public static IEnumerable<(T, int)> Indexed<T>(this IEnumerable<T> source)
        {
            foreach (var pair in source.Select((e, i) => (e, i)))
            {
                yield return pair;
            }
        }

        /// <summary>
        /// Returns null when <paramref name="source"/> is empty, otherwise returns <paramref name="source"/> as a <see cref="List{T}"/>.
        /// </summary>
        public static List<T> NullIfEmpty<T>(this IEnumerable<T> source)
        {
            if (source.Any())
            {
                return source.ToList();
            }
            else
            {
                return null;
            }
        }
    }
}

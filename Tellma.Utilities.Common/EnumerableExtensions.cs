using System;
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


        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, null);
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            comparer ??= Comparer<TKey>.Default;

            using var sourceIterator = source.GetEnumerator();
            if (!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            TSource max = sourceIterator.Current;
            TKey maxKey = selector(max);
            while (sourceIterator.MoveNext())
            {
                var candidate = sourceIterator.Current;
                var candidateKey = selector(candidate);
                if (comparer.Compare(candidateKey, maxKey) > 0)
                {
                    max = candidate;
                    maxKey = candidateKey;
                }
            }

            return max;
        }
    }
}

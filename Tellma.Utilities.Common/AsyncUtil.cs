using System.Collections.Generic;

namespace Tellma.Utilities.Common
{
    /// <summary>
    /// Contains helper methods for creating <see cref="IAsyncEnumerable{T}"/> inside synchronous methods.
    /// </summary>
    public static class AsyncUtil
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously.

        /// <summary>
        /// Creates an empty <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        public static async IAsyncEnumerable<T> Empty<T>()
#pragma warning restore CS1998
        {
            yield break;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously.

        /// <summary>
        /// Creates an <see cref="IAsyncEnumerable{T}"/> containing one <paramref name="item"/>.
        /// </summary>
        public static async IAsyncEnumerable<T> Singleton<T>(T item)
#pragma warning restore CS1998
        {
            yield return item;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously.

        /// <summary>
        /// Creates an <see cref="IAsyncEnumerable{T}"/> containing all of <paramref name="items"/>.
        /// </summary>
        public static async IAsyncEnumerable<T> Multiple<T>(IEnumerable<T> items)
#pragma warning restore CS1998
        {
            foreach (var item in items)
            {
                yield return item;
            }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace Tellma.Services.Utilities
{
    public static class HeaderExtensions
    {
        /// <summary>
        /// Adds the <paramref name="key"/> and <paramref name="value"/> pair to the headers overriding any existing value.
        /// </summary>
        public static void Set(this IHeaderDictionary dic, string key, StringValues value)
        {
            if (dic is null)
            {
                throw new ArgumentNullException(nameof(dic));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }

            dic.Remove(key);
            dic.Add(key, value);
        }

        /// <summary>
        /// Adds the <paramref name="key"/> and <paramref name="value"/> pair to the headers only if <paramref name="key"/> isn't already there.
        /// </summary>
        public static void TrySet(this IHeaderDictionary dic, string key, StringValues value)
        {
            if (dic is null)
            {
                throw new ArgumentNullException(nameof(dic));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }

            if (!dic.ContainsKey(key))
            {
                dic.Add(key, value);
            }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace Tellma.Services.Utilities
{
    public static class HeaderExtensions
    {
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
    }
}

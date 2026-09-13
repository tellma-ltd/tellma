// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>What one call returned, and what the vendor said about pacing while returning it.</summary>
    /// <remarks>
    ///     The quota headers ride on every response, not only on a refusal, and by the time a
    ///     refusal arrives the caller has already been throttled. Carrying them on the result of
    ///     every call is what lets a caller pace itself before that happens; putting them on the
    ///     client instead would be a shared mutable field that lies the moment two calls overlap.
    /// </remarks>
    /// <typeparam name="T">What the call returned.</typeparam>
    /// <param name="Value">What the call returned.</param>
    /// <param name="StatusCode">The status the vendor answered with.</param>
    /// <param name="RateLimit">What the vendor said about the remaining quota.</param>
    public sealed record MarminAeResponse<T>(T Value, int StatusCode, MarminAeRateLimit RateLimit);

    /// <summary>What the vendor last said about the caller's remaining quota.</summary>
    /// <remarks>
    ///     Every member is optional, because a proxy or an error path may strip or mangle the
    ///     headers, and a quota reading that throws would be worse than one that is simply absent.
    /// </remarks>
    public sealed record MarminAeRateLimit
    {
        /// <summary>The reading for a response that carried no quota headers at all.</summary>
        public static MarminAeRateLimit None { get; } = new MarminAeRateLimit();

        /// <summary>How many calls the current window allows.</summary>
        public int? Limit { get; init; }

        /// <summary>How many of them are left.</summary>
        public int? Remaining { get; init; }

        /// <summary>When the window starts over.</summary>
        public DateTimeOffset? ResetsAt { get; init; }

        /// <summary>How long the vendor asked the caller to wait before trying again.</summary>
        public TimeSpan? RetryAfter { get; init; }

        /// <summary>Whether the response carried any quota reading at all.</summary>
        public bool HasValue => Limit is not null || Remaining is not null || ResetsAt is not null
            || RetryAfter is not null;

        /// <summary>Reads the quota headers off a response.</summary>
        /// <param name="headers">The response headers.</param>
        /// <param name="now">The current instant, for a retry hint given as a date.</param>
        /// <returns>What could be read, which may be nothing.</returns>
        internal static MarminAeRateLimit From(HttpResponseHeaders headers, DateTimeOffset now)
        {
            // Three spellings, because the vendor's reference names one set and its deployment sends
            // two others. Reading only the documented ones yields a quota reading that is silently
            // always absent, which is worse than no reading at all: it looks like head-room.
            int? limit = ReadInt32(
                headers,
                MarminAeClient.RateLimitLimitHeaderName,
                MarminAeClient.StandardRateLimitLimitHeaderName,
                MarminAeClient.RateLimitLimitPerMinuteHeaderName);
            int? remaining = ReadInt32(
                headers,
                MarminAeClient.RateLimitRemainingHeaderName,
                MarminAeClient.StandardRateLimitRemainingHeaderName,
                MarminAeClient.RateLimitRemainingPerMinuteHeaderName);
            long? reset = ReadInt64(
                headers,
                MarminAeClient.RateLimitResetHeaderName,
                MarminAeClient.StandardRateLimitResetHeaderName);

            TimeSpan? retryAfter = null;
            RetryConditionHeaderValue? condition = headers.RetryAfter;
            if (condition?.Delta is TimeSpan delta)
            {
                retryAfter = delta;
            }
            else if (condition?.Date is DateTimeOffset date)
            {
                // A retry hint given as an instant is more useful to a caller as a wait, and never
                // as a negative one: a clock skew must not turn into a busy loop.
                TimeSpan wait = date - now;
                retryAfter = wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
            }

            MarminAeRateLimit result = new()
            {
                Limit = limit,
                Remaining = remaining,
                ResetsAt = ResolveReset(reset, now),
                RetryAfter = retryAfter,
            };

            return result.HasValue ? result : None;
        }

        /// <summary>
        ///     Reads the reset hint, which arrives either as an instant or as a wait depending on
        ///     which spelling of the header carried it.
        /// </summary>
        private static DateTimeOffset? ResolveReset(long? reset, DateTimeOffset now)
        {
            const long EpochThreshold = 1_000_000_000L;

            if (reset is not long value || value < 0)
            {
                return null;
            }

            // A value large enough to be an instant is one; anything smaller is the number of
            // seconds until the window turns over, which is what the deployment actually sends.
            return value >= EpochThreshold
                ? DateTimeOffset.FromUnixTimeSeconds(value)
                : now.AddSeconds(value);
        }

        private static int? ReadInt32(HttpResponseHeaders headers, params string[] names)
        {
            foreach (string name in names)
            {
                if (int.TryParse(
                    ReadFirst(headers, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                {
                    return parsed;
                }
            }

            return null;
        }

        private static long? ReadInt64(HttpResponseHeaders headers, params string[] names)
        {
            foreach (string name in names)
            {
                if (long.TryParse(
                    ReadFirst(headers, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed))
                {
                    return parsed;
                }
            }

            return null;
        }

        private static string? ReadFirst(HttpResponseHeaders headers, string name)
        {
            return headers.TryGetValues(name, out IEnumerable<string>? values)
                ? values.FirstOrDefault()
                : null;
        }
    }

    /// <summary>One page of a listing.</summary>
    /// <typeparam name="T">What the page holds.</typeparam>
    public sealed record MarminAePage<T>
    {
        /// <summary>The page's contents.</summary>
        [JsonPropertyName("content")]
        public IReadOnlyList<T>? Content { get; init; }

        /// <summary>Which page this is, counting from zero.</summary>
        [JsonPropertyName("page_number")]
        public int? PageNumber { get; init; }

        /// <summary>How many pages the query has.</summary>
        [JsonPropertyName("total_pages")]
        public int? TotalPages { get; init; }

        /// <summary>How many results the query has in total.</summary>
        [JsonPropertyName("total_elements")]
        public long? TotalElements { get; init; }
    }
}

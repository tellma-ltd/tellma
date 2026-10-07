// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Text.Json;

namespace Tellma.Connector.MarminAe
{
    /// <summary>Reads the instants the vendor sends without publishing a type for them.</summary>
    /// <remarks>
    ///     <para>
    ///         The token response carries an expiry whose encoding the vendor reference does not
    ///         state, so every plausible one is accepted: epoch seconds, epoch milliseconds, either
    ///         of those delivered as a JSON string, and an ISO-8601 instant.
    ///     </para>
    ///     <para>
    ///         An ISO-8601 value with no zone designator is read as UTC rather than as the vendor's
    ///         Gulf Standard Time. The asymmetry is deliberate: reading a Gulf timestamp as UTC
    ///         makes a token look four hours older than it is and refreshes early, which costs one
    ///         request, while the opposite mistake makes it look four hours younger and sends a
    ///         dead token on every call.
    ///     </para>
    ///     <para>
    ///         Nothing here throws. Every path yields null for a value it cannot make sense of,
    ///         because an unreadable expiry is a reason to refresh sooner, not a reason to fail.
    ///     </para>
    /// </remarks>
    internal static class MarminAeTimestamps
    {
        // Roughly 2001 in seconds and 2001 in milliseconds. A value below the seconds threshold
        // cannot be a sane absolute instant, so it is read as a relative lifetime instead.
        private const long MillisecondsThreshold = 1_000_000_000_000L;
        private const long SecondsThreshold = 1_000_000_000L;

        /// <summary>Reads an instant out of whatever JSON shape carried it.</summary>
        /// <param name="element">The value.</param>
        /// <param name="now">The current instant, for a value expressed as a lifetime.</param>
        /// <returns>The instant, or null when the value could not be read as one.</returns>
        internal static DateTimeOffset? Interpret(JsonElement element, DateTimeOffset now)
        {
            DateTimeOffset? result = null;

            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out long number))
            {
                result = FromNumber(number, now);
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                string? text = element.GetString();
                result = string.IsNullOrWhiteSpace(text) ? null : FromText(text, now);
            }

            return result;
        }

        /// <summary>Reads a numeric instant, disambiguating its unit by magnitude.</summary>
        /// <param name="value">The number.</param>
        /// <param name="now">The current instant, for a value expressed as a lifetime.</param>
        /// <returns>The instant, or null when the number is not usable.</returns>
        internal static DateTimeOffset? FromNumber(long value, DateTimeOffset now)
        {
            if (value <= 0)
            {
                return null;
            }

            DateTimeOffset result = value >= MillisecondsThreshold
                ? DateTimeOffset.FromUnixTimeMilliseconds(value)
                : value >= SecondsThreshold
                    ? DateTimeOffset.FromUnixTimeSeconds(value)
                    : now.AddSeconds(value);

            return result;
        }

        /// <summary>Reads a textual instant, numeric or ISO-8601.</summary>
        /// <param name="value">The text.</param>
        /// <param name="now">The current instant, for a value expressed as a lifetime.</param>
        /// <returns>The instant, or null when the text could not be read as one.</returns>
        internal static DateTimeOffset? FromText(string value, DateTimeOffset now)
        {
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long number))
            {
                return FromNumber(number, now);
            }

            bool parsed = DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal,
                out DateTimeOffset instant);

            return parsed ? instant : null;
        }

        /// <summary>Reads the expiry claim out of a JWT, without validating the token.</summary>
        /// <remarks>
        ///     The authority of last resort when the envelope around the token is unreadable: the
        ///     token states its own lifetime, and nothing here trusts that statement for anything
        ///     beyond deciding when to ask for the next one.
        /// </remarks>
        /// <param name="token">The compact-serialized JWT.</param>
        /// <returns>The expiry, or null when the token carries no readable one.</returns>
        internal static DateTimeOffset? FromJwtExpiry(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            int firstDot = token.IndexOf('.');
            int secondDot = firstDot < 0 ? -1 : token.IndexOf('.', firstDot + 1);
            if (secondDot <= firstDot + 1)
            {
                return null;
            }

            ReadOnlySpan<char> payload = token.AsSpan(firstDot + 1, secondDot - firstDot - 1);

            byte[] decoded;
            try
            {
                decoded = DecodeBase64Url(payload);
            }
            catch (FormatException)
            {
                return null;
            }

            long seconds = 0;
            try
            {
                using var document = JsonDocument.Parse(decoded);
                bool readable = document.RootElement.ValueKind == JsonValueKind.Object
                    && document.RootElement.TryGetProperty("exp", out JsonElement expiry)
                    && expiry.TryGetInt64(out seconds)
                    && seconds > 0;

                return readable ? DateTimeOffset.FromUnixTimeSeconds(seconds) : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        ///     Decodes base64url (RFC 4648 section 5) into bytes.
        /// </summary>
        /// <remarks>
        ///     Stands in for <c>System.Buffers.Text.Base64Url</c>, which is .NET 9+ while this
        ///     project targets net8.0. Base64url differs from base64 in exactly two ways: it
        ///     substitutes <c>-</c> for <c>+</c> and <c>_</c> for <c>/</c>, and it drops the
        ///     trailing <c>=</c> padding. Both are reversed here before handing the result to
        ///     <see cref="Convert.FromBase64String" />.
        /// </remarks>
        /// <param name="value">The base64url text, with or without padding.</param>
        /// <returns>The decoded bytes.</returns>
        /// <exception cref="FormatException">The input is not well-formed base64url.</exception>
        internal static byte[] DecodeBase64Url(ReadOnlySpan<char> value)
        {
            // A JWT payload is small, so the copy costs nothing worth optimising away.
            char[] buffer = new char[value.Length + 3];
            for (int i = 0; i < value.Length; i++)
            {
                buffer[i] = value[i] switch
                {
                    '-' => '+',
                    '_' => '/',

                    // Anything else is passed through so that a stray character fails in
                    // Convert.FromBase64String as a FormatException, which the caller expects.
                    char other => other,
                };
            }

            // Base64 decodes in 4-character groups, so restore however much padding was stripped.
            // A remainder of 1 is not reachable from valid base64url; leaving it unpadded makes
            // Convert.FromBase64String reject it, which is the intended outcome.
            int length = value.Length;
            int remainder = length % 4;
            if (remainder == 2 || remainder == 3)
            {
                int padding = 4 - remainder;
                for (int i = 0; i < padding; i++)
                {
                    buffer[length++] = '=';
                }
            }

            return Convert.FromBase64String(new string(buffer, 0, length));
        }

    }
}

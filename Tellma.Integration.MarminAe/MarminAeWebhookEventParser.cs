// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

namespace Tellma.Connector.MarminAe
{
    /// <summary>Reads a webhook notification.</summary>
    /// <remarks>
    ///     Hand-rolled over <see cref="JsonDocument" /> rather than source-generated, because a
    ///     notification has exactly seven fields and every one of them has to be present and
    ///     well-formed for the notification to be actionable at all. Reading the object directly
    ///     makes "could not be read" a return value rather than an exception, which is what a
    ///     handler on an anonymous endpoint needs.
    /// </remarks>
    public static class MarminAeWebhookEventParser
    {
        /// <summary>Reads a verified payload.</summary>
        /// <remarks>
        ///     Takes memory rather than a span so the document is parsed over the caller's own
        ///     buffer instead of a copy of it — the same buffer the signature was checked against.
        ///     Fields the vendor adds later are ignored.
        /// </remarks>
        /// <param name="body">The request body.</param>
        /// <param name="event">The notification, when the body carried one.</param>
        /// <returns>True when the body was a notification this client can act on.</returns>
        public static bool TryParse(ReadOnlyMemory<byte> body, [NotNullWhen(true)] out MarminAeWebhookEvent? @event)
        {
            @event = null;

            if (body.IsEmpty)
            {
                return false;
            }

            try
            {
                // A byte-order mark is not JSON and the reader will not skip one. It is signed
                // along with everything else, so verification is unaffected, but leaving it here
                // would turn a body some proxy decided to mark into an unparseable one.
                using var document = JsonDocument.Parse(TrimByteOrderMark(body));
                JsonElement root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                string? eventType = ReadText(root, "event_type");
                string? profileId = ReadText(root, "profile_id");

                if (string.IsNullOrWhiteSpace(eventType)
                    || !TryReadGuid(root, "org_id", out Guid orgId)
                    || !TryReadGuid(root, "resource_id", out Guid resourceId)
                    || !TryReadGuid(root, "webhook_event_id", out Guid webhookEventId)
                    || !TryReadUri(root, "resource_url", out Uri? resourceUrl)
                    || !TryReadTimestamp(root, "event_timestamp", out DateTimeOffset eventTimestamp))
                {
                    return false;
                }

                @event = new MarminAeWebhookEvent(
                    orgId,
                    eventType,
                    profileId ?? string.Empty,
                    resourceId,
                    resourceUrl,
                    eventTimestamp,
                    webhookEventId);

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static ReadOnlyMemory<byte> TrimByteOrderMark(ReadOnlyMemory<byte> body)
        {
            ReadOnlySpan<byte> mark = [0xEF, 0xBB, 0xBF];

            return body.Length >= mark.Length && body.Span[..mark.Length].SequenceEqual(mark)
                ? body[mark.Length..]
                : body;
        }

        private static string? ReadText(JsonElement root, string name)
        {
            return root.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static bool TryReadGuid(JsonElement root, string name, out Guid value)
        {
            value = Guid.Empty;
            string? text = ReadText(root, name);

            return !string.IsNullOrWhiteSpace(text) && Guid.TryParse(text, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryReadUri(JsonElement root, string name, [NotNullWhen(true)] out Uri? value)
        {
            string? text = ReadText(root, name);
            value = null;

            // Absolute is not a strong enough test on its own. A Unix host reads an absolute-looking
            // path as a file URI, so a relative link in a delivery would be accepted here and handed
            // to a caller as somewhere to fetch from — and the payload it came in is attacker-shaped
            // until the signature says otherwise. The scheme has to be one the vendor can serve.
            if (string.IsNullOrWhiteSpace(text)
                || !Uri.TryCreate(text, UriKind.Absolute, out Uri? parsed)
                || (parsed.Scheme != Uri.UriSchemeHttps && parsed.Scheme != Uri.UriSchemeHttp))
            {
                return false;
            }

            value = parsed;

            return true;
        }

        private static bool TryReadTimestamp(JsonElement root, string name, out DateTimeOffset value)
        {
            value = default;

            if (!root.TryGetProperty(name, out JsonElement element))
            {
                return false;
            }

            // A delivery's timestamp is documented as an instant in ISO-8601, but the same tolerant
            // reading the token expiry gets costs nothing and covers an epoch just as well.
            DateTimeOffset? parsed = MarminAeTimestamps.Interpret(element, DateTimeOffset.UnixEpoch);
            if (parsed is null)
            {
                return false;
            }

            value = parsed.Value;

            return true;
        }
    }
}

// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json;

namespace Tellma.Connector.MarminAe
{
    /// <summary>Reads whatever can be read out of a refusal body.</summary>
    /// <remarks>
    ///     <para>
    ///         The vendor publishes no schema for its error bodies, and the shapes it has been
    ///         observed using are not one shape. Rather than guess at one and lose everything that
    ///         does not match it, this recognises the shapes seen so far and keeps the body
    ///         verbatim regardless.
    ///     </para>
    ///     <para>
    ///         Nothing here throws. A parser that failed on an unexpected refusal body would
    ///         replace a diagnosable vendor error with an undiagnosable client one, at the exact
    ///         moment somebody needs to know what the vendor said.
    ///     </para>
    /// </remarks>
    internal static class MarminAeErrorParser
    {
        /// <summary>
        ///     How much of a refusal body is worth keeping. Long enough for any structured error
        ///     the vendor produces, short enough that a runaway HTML page does not end up in a log.
        /// </summary>
        internal const int MaxRawBodyLength = 8 * 1024;

        private const string TruncationMarker = "… [truncated]";

        /// <summary>Reads a refusal body.</summary>
        /// <param name="body">The body as it arrived.</param>
        /// <returns>What could be read, alongside the body itself.</returns>
        internal static MarminAeErrorDetail Parse(string? body)
        {
            bool truncated = body is not null && body.Length > MaxRawBodyLength;

            // Cutting at a fixed index can land between the halves of a surrogate pair, leaving one
            // behind — which a logger either replaces with a question mark or refuses outright.
            int keep = truncated && char.IsHighSurrogate(body![MaxRawBodyLength - 1])
                ? MaxRawBodyLength - 1
                : MaxRawBodyLength;

            string? raw = body is null
                ? null
                : truncated
                    ? string.Concat(body.AsSpan(0, keep), TruncationMarker)
                    : body;

            if (string.IsNullOrWhiteSpace(body))
            {
                return new MarminAeErrorDetail(null, [], raw, truncated);
            }

            try
            {
                // Parsed over the whole body rather than the truncated capture: cutting a document
                // in half would make a readable error unreadable.
                using var document = JsonDocument.Parse(body);
                JsonElement root = document.RootElement;

                if (root.ValueKind == JsonValueKind.String)
                {
                    return new MarminAeErrorDetail(root.GetString(), [], raw, truncated);
                }

                if (root.ValueKind != JsonValueKind.Object)
                {
                    return new MarminAeErrorDetail(null, [], raw, truncated);
                }

                List<MarminAeError> errors = [];
                string? message = null;

                if (root.TryGetProperty("errors", out JsonElement wrapped))
                {
                    message = ReadWrapped(wrapped, errors);
                }

                message ??= ReadFirstText(root, "message", "error", "detail", "title", "errorMessage");

                return new MarminAeErrorDetail(message, errors, raw, truncated);
            }
            catch (JsonException)
            {
                // Not JSON at all — a gateway page, a truncated response, a bare stack trace. The
                // capture above is the whole of what this can offer, and it is the useful part.
                return new MarminAeErrorDetail(null, [], raw, truncated);
            }
        }

        /// <summary>Reads whatever sits under the wrapper the vendor puts errors in.</summary>
        private static string? ReadWrapped(JsonElement wrapped, List<MarminAeError> errors)
        {
            if (wrapped.ValueKind == JsonValueKind.String)
            {
                return wrapped.GetString();
            }

            if (wrapped.ValueKind == JsonValueKind.Object)
            {
                return ReadErrorObject(wrapped, errors);
            }

            if (wrapped.ValueKind == JsonValueKind.Array)
            {
                ReadErrorArray(wrapped, errors);
            }

            return null;
        }

        /// <summary>
        ///     Reads an object of errors, which is either a single described refusal or a map from
        ///     payload path to what was wrong with it.
        /// </summary>
        private static string? ReadErrorObject(JsonElement wrapped, List<MarminAeError> errors)
        {
            string? message = null;

            foreach (JsonProperty property in wrapped.EnumerateObject())
            {
                if (IsMessageName(property.Name) && property.Value.ValueKind == JsonValueKind.String)
                {
                    message = property.Value.GetString();
                    continue;
                }

                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    errors.Add(new MarminAeError(property.Value.GetString(), property.Name));
                }
                else if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    errors.Add(ReadDescribedError(property.Value, fallbackField: property.Name));
                }
                else if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            errors.Add(new MarminAeError(item.GetString(), property.Name));
                        }
                    }
                }
            }

            return message;
        }

        /// <summary>Reads an array of errors, described or bare.</summary>
        private static void ReadErrorArray(JsonElement wrapped, List<MarminAeError> errors)
        {
            foreach (JsonElement item in wrapped.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    errors.Add(new MarminAeError(item.GetString()));
                }
                else if (item.ValueKind == JsonValueKind.Object)
                {
                    errors.Add(ReadDescribedError(item, fallbackField: null));
                }
            }
        }

        /// <summary>Reads one described error, under whichever names it used.</summary>
        private static MarminAeError ReadDescribedError(JsonElement item, string? fallbackField)
        {
            string? message = ReadFirstText(item, "message", "defaultMessage", "detail", "error", "description");
            string? field = ReadFirstText(item, "field", "path", "property", "propertyPath", "location")
                ?? fallbackField;
            string? code = ReadFirstText(item, "code", "errorCode", "reasonCode");

            return new MarminAeError(message, field, code);
        }

        private static string? ReadFirstText(JsonElement element, params string[] names)
        {
            foreach (string name in names)
            {
                if (element.TryGetProperty(name, out JsonElement value)
                    && value.ValueKind == JsonValueKind.String)
                {
                    string? text = value.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }

            return null;
        }

        private static bool IsMessageName(string name)
        {
            return string.Equals(name, "message", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "detail", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "title", StringComparison.OrdinalIgnoreCase);
        }
    }
}

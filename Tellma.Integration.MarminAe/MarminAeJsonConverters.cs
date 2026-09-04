// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>
    ///     Writes an optional date in the vendor's format and reads one back without failing the
    ///     whole document over its formatting.
    /// </summary>
    /// <remarks>
    ///     The framework converter accepts exactly ten characters of ISO-8601 and throws on
    ///     anything else, so one date echoed back as a full timestamp would take an entire read
    ///     with it. Reading is therefore forgiving and yields null rather than throwing; writing
    ///     stays exact, because sending a malformed date is this client's own bug.
    /// </remarks>
    internal sealed class MarminAeDateOnlyConverter : JsonConverter<DateOnly?>
    {
        /// <inheritdoc />
        public override bool HandleNull => true;

        /// <inheritdoc />
        public override DateOnly? Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                return null;
            }

            string? text = reader.GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (DateOnly.TryParseExact(
                text,
                MarminAeRoutes.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly exact))
            {
                return exact;
            }

            // A date delivered as a timestamp: keep the calendar day and drop the rest, which is
            // all a field typed as a date was ever meant to carry.
            return DateTimeOffset.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal,
                out DateTimeOffset parsed)
                ? DateOnly.FromDateTime(parsed.Date)
                : null;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(writer);

            if (value is DateOnly date)
            {
                writer.WriteStringValue(
                    date.ToString(MarminAeRoutes.DateFormat, CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    /// <summary>
    ///     Writes an optional time of day exactly as the vendor requires, and reads one back
    ///     leniently.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The vendor accepts <c>HH:mm:ss</c> in Gulf Standard Time and nothing longer. The
    ///         framework converter emits fractional seconds whenever a value carries them, and
    ///         <see cref="TimeOnly.FromDateTime(DateTime)" /> — the obvious way for a caller to
    ///         reach a time of day — carries them. Truncating to the second here turns a validation
    ///         failure the caller could not see coming into a non-event.
    ///     </para>
    ///     <para>
    ///         Reading accepts anything <see cref="TimeOnly.TryParse(string, IFormatProvider, DateTimeStyles, out TimeOnly)" />
    ///         does, including the <c>HH:mm</c> the vendor has been observed echoing, and yields
    ///         null for anything else.
    ///     </para>
    /// </remarks>
    internal sealed class MarminAeTimeOnlyConverter : JsonConverter<TimeOnly?>
    {
        private const string WireFormat = "HH:mm:ss";

        /// <inheritdoc />
        public override bool HandleNull => true;

        /// <inheritdoc />
        public override TimeOnly? Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                return null;
            }

            string? text = reader.GetString();

            return TimeOnly.TryParse(
                text, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly parsed)
                ? parsed
                : null;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(writer);

            if (value is TimeOnly time)
            {
                writer.WriteStringValue(time.ToString(WireFormat, CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}

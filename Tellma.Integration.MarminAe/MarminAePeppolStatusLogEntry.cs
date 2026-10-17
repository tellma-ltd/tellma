// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>One step in a document's journey through the exchange network.</summary>
    /// <remarks>
    ///     The log is the chronological counterpart to the status snapshot, and — unlike the
    ///     snapshot — the vendor names its fields in snake case. The event names are not published
    ///     exhaustively, so <see cref="Event" /> stays a string.
    /// </remarks>
    public sealed record MarminAePeppolStatusLogEntry
    {
        /// <summary>The vendor's identifier for this log entry.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The message this step concerned, once one had been sent.</summary>
        [JsonPropertyName("sent_message_id")]
        public string? SentMessageId { get; init; }

        /// <summary>The document the log belongs to.</summary>
        [JsonPropertyName("document_uuid")]
        public string? DocumentUuid { get; init; }

        /// <summary>Which kind of document it is.</summary>
        [JsonPropertyName("document_type")]
        public string? DocumentType { get; init; }

        /// <summary>Who sent it.</summary>
        [JsonPropertyName("sender_id")]
        public string? SenderId { get; init; }

        /// <summary>Who received it.</summary>
        [JsonPropertyName("receiver_id")]
        public string? ReceiverId { get; init; }

        /// <summary>Which document type was transmitted, in the network's own terms.</summary>
        [JsonPropertyName("doc_type_id")]
        public string? DocumentTypeId { get; init; }

        /// <summary>Which process the transmission ran under.</summary>
        [JsonPropertyName("process_id")]
        public string? ProcessId { get; init; }

        /// <summary>The country of the first corner.</summary>
        [JsonPropertyName("country_c1")]
        public string? CountryC1 { get; init; }

        /// <summary>What was transmitted at this step.</summary>
        [JsonPropertyName("document_xml")]
        public string? DocumentXml { get; init; }

        /// <summary>What happened, in prose.</summary>
        [JsonPropertyName("message")]
        public string? Message { get; init; }

        /// <summary>When it happened, in Unix milliseconds.</summary>
        [JsonPropertyName("timestamp")]
        public long? TimestampMilliseconds { get; init; }

        /// <summary>When it happened.</summary>
        [JsonIgnore]
        public DateTimeOffset? Timestamp => TimestampMilliseconds is long value
            ? DateTimeOffset.FromUnixTimeMilliseconds(value)
            : null;

        /// <summary>What happened, as a code.</summary>
        [JsonPropertyName("event")]
        public string? Event { get; init; }
    }
}

// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>The outcomes a document's transmission can report.</summary>
    /// <remarks>
    ///     The vendor does not publish the vocabulary exhaustively, so these are the values worth
    ///     branching on rather than the whole set. Every status on every model stays a string, and
    ///     an unrecognised one passes through untouched.
    /// </remarks>
    public static class MarminAePeppolStatus
    {
        /// <summary>The document failed validation and may be corrected and resubmitted.</summary>
        public const string ValidationFailed = "VALIDATION_FAILED";

        /// <summary>The document is still being processed.</summary>
        public const string Pending = "PENDING";

        /// <summary>The document was accepted.</summary>
        public const string Approved = "APPROVED";

        /// <summary>The document was processed and refused.</summary>
        public const string Rejected = "REJECTED";
    }

    /// <summary>Where a document currently stands on each transmission leg.</summary>
    /// <remarks>
    ///     <para>
    ///         A snapshot rather than a history, and a different shape from the summary carried on
    ///         the document itself: this one names the two legs separately — delivery to the
    ///         buyer's provider, and reporting to the authority — and each carries its own
    ///         transmission detail.
    ///     </para>
    ///     <para>
    ///         The vendor names these fields in camel case, alone among the endpoints this client
    ///         covers, which is why every member here spells its wire name out.
    ///     </para>
    /// </remarks>
    public sealed record MarminAePeppolStatusSnapshot
    {
        /// <summary>Which kind of document this is.</summary>
        [JsonPropertyName("documentType")]
        public string? DocumentType { get; init; }

        /// <summary>The document this snapshot is about.</summary>
        [JsonPropertyName("documentUuid")]
        public string? DocumentUuid { get; init; }

        /// <summary>The document as transmitted, Base64-encoded.</summary>
        [JsonPropertyName("documentXML")]
        public string? DocumentXml { get; init; }

        /// <summary>The vendor's identifier for this status record.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The status record's own identifier, distinct from the document's.</summary>
        [JsonPropertyName("uuid")]
        public string? Uuid { get; init; }

        /// <summary>Who sent the document, as a network participant.</summary>
        [JsonPropertyName("issuedBy")]
        public string? IssuedBy { get; init; }

        /// <summary>Who it was sent to, as a network participant.</summary>
        [JsonPropertyName("issuedTo")]
        public string? IssuedTo { get; init; }

        /// <summary>When the snapshot last changed, in Unix milliseconds.</summary>
        [JsonPropertyName("lastUpdated")]
        public long? LastUpdatedMilliseconds { get; init; }

        /// <summary>When the snapshot last changed.</summary>
        [JsonIgnore]
        public DateTimeOffset? LastUpdated => LastUpdatedMilliseconds is long value
            ? DateTimeOffset.FromUnixTimeMilliseconds(value)
            : null;

        /// <summary>What validation objected to, when it objected.</summary>
        /// <remarks>Left as raw JSON, for the reason the document-level status records.</remarks>
        [JsonPropertyName("validationResults")]
        public IReadOnlyList<JsonElement>? ValidationResults { get; init; }

        /// <summary>The leg delivering the document to the buyer's provider.</summary>
        [JsonPropertyName("toC3")]
        public MarminAePeppolLeg? ToC3 { get; init; }

        /// <summary>The leg reporting the document to the authority.</summary>
        [JsonPropertyName("toC5")]
        public MarminAePeppolLeg? ToC5 { get; init; }
    }

    /// <summary>One leg of a document's transmission.</summary>
    public sealed record MarminAePeppolLeg
    {
        /// <summary>The country this leg runs in.</summary>
        [JsonPropertyName("country")]
        public string? Country { get; init; }

        /// <summary>Which document type was transmitted, in the network's own terms.</summary>
        [JsonPropertyName("docTypeId")]
        public string? DocumentTypeId { get; init; }

        /// <summary>What was transmitted on this leg, Base64-encoded.</summary>
        [JsonPropertyName("documentXml")]
        public string? DocumentXml { get; init; }

        /// <summary>When this leg last changed, in Unix milliseconds.</summary>
        [JsonPropertyName("lastUpdated")]
        public long? LastUpdatedMilliseconds { get; init; }

        /// <summary>When this leg last changed.</summary>
        [JsonIgnore]
        public DateTimeOffset? LastUpdated => LastUpdatedMilliseconds is long value
            ? DateTimeOffset.FromUnixTimeMilliseconds(value)
            : null;

        /// <summary>The acknowledgement the far end returned, when there was one.</summary>
        /// <remarks>Left as raw JSON: the vendor publishes no shape for it.</remarks>
        [JsonPropertyName("mlsResponse")]
        public JsonElement? MlsResponse { get; init; }

        /// <summary>Which process the transmission ran under.</summary>
        [JsonPropertyName("processId")]
        public string? ProcessId { get; init; }

        /// <summary>Who received this leg.</summary>
        [JsonPropertyName("receiverId")]
        public string? ReceiverId { get; init; }

        /// <summary>Who sent it.</summary>
        [JsonPropertyName("senderId")]
        public string? SenderId { get; init; }

        /// <summary>How this leg went.</summary>
        [JsonPropertyName("status")]
        public string? Status { get; init; }

        /// <summary>The identifier of this transmission attempt.</summary>
        [JsonPropertyName("transmissionUuid")]
        public string? TransmissionUuid { get; init; }

        /// <summary>What the transport itself reported.</summary>
        [JsonPropertyName("transmissionResponse")]
        public MarminAePeppolTransmissionResponse? TransmissionResponse { get; init; }
    }

    /// <summary>What the transport reported about one transmission attempt.</summary>
    public sealed record MarminAePeppolTransmissionResponse
    {
        /// <summary>The conversation the message belonged to.</summary>
        [JsonPropertyName("conversationId")]
        public string? ConversationId { get; init; }

        /// <summary>The message's own identifier.</summary>
        [JsonPropertyName("messageId")]
        public string? MessageId { get; init; }

        /// <summary>How long the attempt took, in milliseconds.</summary>
        [JsonPropertyName("overallDurationToTransmit")]
        public long? OverallDurationToTransmitMilliseconds { get; init; }

        /// <summary>Whether the transport flagged an error.</summary>
        [JsonPropertyName("transmissionError")]
        public bool? TransmissionError { get; init; }

        /// <summary>What went wrong, when something did.</summary>
        /// <remarks>Left as raw JSON: the vendor publishes no shape for these entries.</remarks>
        [JsonPropertyName("transmissionErrors")]
        public IReadOnlyList<JsonElement>? TransmissionErrors { get; init; }

        /// <summary>The transport's own verdict on the attempt.</summary>
        [JsonPropertyName("transmissionResult")]
        public string? TransmissionResult { get; init; }
    }
}

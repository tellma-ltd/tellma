// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>A pointer to another document.</summary>
    /// <remarks>
    ///     The vendor uses this one shape for every document-to-document reference a document
    ///     carries: the invoice a credit note adjusts, and the despatch, receipt, statement,
    ///     originator and contract documents behind it. <see cref="Id" /> is required by the vendor
    ///     wherever the object appears.
    /// </remarks>
    public sealed record MarminAeDocumentReference
    {
        /// <summary>The referenced document's number. Required by the vendor.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The referenced document's date.</summary>
        [JsonPropertyName("issue_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? IssueDate { get; init; }

        /// <summary>A description the vendor returns on some references.</summary>
        [JsonPropertyName("document_description")]
        public string? DocumentDescription { get; init; }
    }

    /// <summary>A pointer to the purchase order behind a document.</summary>
    public sealed record MarminAeOrderReference
    {
        /// <summary>The order number. Required by the vendor.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The order date.</summary>
        [JsonPropertyName("issue_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? IssueDate { get; init; }

        /// <summary>The sales-order identifier on the issuer's side.</summary>
        [JsonPropertyName("sales_order_id")]
        public string? SalesOrderId { get; init; }
    }

    /// <summary>The project a document belongs to.</summary>
    public sealed record MarminAeProjectReference
    {
        /// <summary>The project identifier. Required by the vendor.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The project name.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }

    /// <summary>An identifier standing in for a whole party.</summary>
    /// <remarks>
    ///     The vendor uses this for the two UAE scenario identifiers that are not full parties: the
    ///     beneficiary a free-zone supply names, and the principal a disclosed agent bills on
    ///     behalf of. Which one is required, and when, is tax knowledge this client does not carry.
    /// </remarks>
    public sealed record MarminAePartyReference
    {
        /// <summary>The identifier.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }
    }

    /// <summary>The line of an order a document line answers.</summary>
    public sealed record MarminAeOrderLineReference
    {
        /// <summary>The line identifier. Required by the vendor when the object is present.</summary>
        [JsonPropertyName("line_id")]
        public string? LineId { get; init; }

        /// <summary>The order the line belongs to.</summary>
        [JsonPropertyName("order_reference_id")]
        public string? OrderReferenceId { get; init; }
    }

    /// <summary>The line of a despatch document a document line answers.</summary>
    public sealed record MarminAeDespatchLineReference
    {
        /// <summary>The line identifier. Required by the vendor when the object is present.</summary>
        [JsonPropertyName("line_id")]
        public string? LineId { get; init; }
    }

    /// <summary>An item identifier assigned by one of the trading parties.</summary>
    public sealed record MarminAeItemIdentification
    {
        /// <summary>The identifier. Required by the vendor when the object is present.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }
    }

    /// <summary>An item identifier from a registered scheme.</summary>
    public sealed record MarminAeStandardItemIdentification
    {
        /// <summary>The identifier. Required by the vendor when the object is present.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The scheme the identifier belongs to. Required alongside it.</summary>
        [JsonPropertyName("scheme_id")]
        public string? SchemeId { get; init; }
    }

    /// <summary>A further item identifier, used for the service accounting code.</summary>
    public sealed record MarminAeAdditionalItemIdentification
    {
        /// <summary>The identifier. Required by the vendor when the object is present.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The scheme the identifier belongs to. Required alongside it.</summary>
        [JsonPropertyName("scheme_id")]
        public string? SchemeId { get; init; }

        /// <summary>The scheme's version.</summary>
        [JsonPropertyName("scheme_version_id")]
        public string? SchemeVersionId { get; init; }
    }

    /// <summary>What a line is selling, and how it is classified.</summary>
    /// <remarks>
    ///     Which members are obliged depends on whether the line carries goods, services, or both
    ///     — a determination that belongs to the layer that knows the supply, not here.
    /// </remarks>
    public sealed record MarminAeCommodityClassification
    {
        /// <summary>Whether the line carries goods, services, or both.</summary>
        [JsonPropertyName("commodity_code")]
        public string? CommodityCode { get; init; }

        /// <summary>The harmonized-system code, for a line carrying goods.</summary>
        [JsonPropertyName("item_classification_code")]
        public string? ItemClassificationCode { get; init; }

        /// <summary>The scheme the classification code belongs to.</summary>
        [JsonPropertyName("item_classification_list_id")]
        public string? ItemClassificationListId { get; init; }

        /// <summary>The scheme's version.</summary>
        [JsonPropertyName("item_classification_list_version_id")]
        public string? ItemClassificationListVersionId { get; init; }

        /// <summary>The nature of a supply the reverse charge applies to.</summary>
        [JsonPropertyName("nature_code")]
        public string? NatureCode { get; init; }
    }

    /// <summary>Where the goods on a line came from.</summary>
    public sealed record MarminAeOriginCountry
    {
        /// <summary>The ISO 3166-1 alpha-2 country code.</summary>
        [JsonPropertyName("identification_code")]
        public string? IdentificationCode { get; init; }
    }

    /// <summary>An attribute of what a line is selling.</summary>
    public sealed record MarminAeAdditionalItemProperty
    {
        /// <summary>The attribute's name. Required by the vendor when the object is present.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>The attribute's value. Required alongside the name.</summary>
        [JsonPropertyName("value")]
        public string? Value { get; init; }
    }
}

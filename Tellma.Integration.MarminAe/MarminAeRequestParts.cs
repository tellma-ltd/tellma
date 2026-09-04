// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>The customer a sales document is issued to.</summary>
    /// <remarks>
    ///     <para>
    ///         There is deliberately no supplier counterpart. The vendor derives the supplier from
    ///         the business profile named in the route and overwrites anything sent, so this client
    ///         has no way to express one — which is the point.
    ///     </para>
    ///     <para>
    ///         The members declared required here are the ones the vendor requires of every party
    ///         on every document. The conditional ones depend on where the party is established and
    ///         whether it is a business, which is a determination the layer above makes.
    ///     </para>
    /// </remarks>
    public sealed record MarminAePartyRequest
    {
        /// <summary>The party's display name.</summary>
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        /// <summary>The party's postal address.</summary>
        [JsonPropertyName("postal_address")]
        public required MarminAeAddress PostalAddress { get; init; }

        /// <summary>The party's email address.</summary>
        [JsonPropertyName("email")]
        public required string Email { get; init; }

        /// <summary>
        ///     The party's network address, which is how the document is routed to it.
        /// </summary>
        [JsonPropertyName("endpoint_id")]
        public required string EndpointId { get; init; }

        /// <summary>The scheme the network address belongs to.</summary>
        [JsonPropertyName("endpoint_scheme_id")]
        public required string EndpointSchemeId { get; init; }

        /// <summary>The party's legal name, which the vendor requires of a business.</summary>
        [JsonPropertyName("party_name")]
        public string? PartyName { get; init; }

        /// <summary>
        ///     The party's taxpayer identification number, which the vendor requires of a local
        ///     business and rejects on a foreign one.
        /// </summary>
        [JsonPropertyName("tin")]
        public string? Tin { get; init; }

        /// <summary>The party's telephone number.</summary>
        [JsonPropertyName("telephone")]
        public string? Telephone { get; init; }

        /// <summary>Which kind of registration the party is identified by.</summary>
        [JsonPropertyName("scheme_agency_id")]
        public string? SchemeAgencyId { get; init; }

        /// <summary>
        ///     The party's registration number, which the vendor requires alongside a registration
        ///     type. Distinct from the VAT number on <see cref="PartyTaxScheme" />.
        /// </summary>
        [JsonPropertyName("company_id")]
        public string? CompanyId { get; init; }

        /// <summary>The authority behind a trade-licence registration.</summary>
        [JsonPropertyName("authority_name")]
        public string? AuthorityName { get; init; }

        /// <summary>The country behind a passport registration.</summary>
        [JsonPropertyName("passport_issuing_country_code")]
        public string? PassportIssuingCountryCode { get; init; }

        /// <summary>The party's VAT registration.</summary>
        [JsonPropertyName("party_tax_scheme")]
        public MarminAePartyTaxScheme? PartyTaxScheme { get; init; }
    }

    /// <summary>What a line charges per unit.</summary>
    public sealed record MarminAePriceRequest
    {
        /// <summary>The net price of one price base quantity.</summary>
        [JsonPropertyName("base_amount")]
        public required decimal BaseAmount { get; init; }

        /// <summary>
        ///     How many units the price covers, in the same unit of measure as the line.
        /// </summary>
        [JsonPropertyName("base_quantity")]
        public required decimal BaseQuantity { get; init; }

        /// <summary>A discount taken off the unit price.</summary>
        [JsonPropertyName("allowance")]
        public MarminAePriceAllowance? Allowance { get; init; }
    }

    /// <summary>One line of a sales document.</summary>
    public sealed record MarminAeDocumentLineRequest
    {
        /// <summary>What is being sold.</summary>
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        /// <summary>A fuller description of what is being sold.</summary>
        [JsonPropertyName("description")]
        public required string Description { get; init; }

        /// <summary>How much of it.</summary>
        [JsonPropertyName("quantity")]
        public required decimal Quantity { get; init; }

        /// <summary>The unit the quantity is counted in.</summary>
        [JsonPropertyName("unit_code")]
        public required string UnitCode { get; init; }

        /// <summary>What it costs per unit.</summary>
        [JsonPropertyName("price")]
        public required MarminAePriceRequest Price { get; init; }

        /// <summary>How it is treated for VAT.</summary>
        [JsonPropertyName("classified_tax_category")]
        public required MarminAeTaxCategory ClassifiedTaxCategory { get; init; }

        /// <summary>A note shown against the line.</summary>
        [JsonPropertyName("note")]
        public string? Note { get; init; }

        /// <summary>The cost centre the line is booked to.</summary>
        [JsonPropertyName("accounting_cost")]
        public string? AccountingCost { get; init; }

        /// <summary>The period the line covers.</summary>
        [JsonPropertyName("invoice_period")]
        public MarminAeInvoicePeriod? InvoicePeriod { get; init; }

        /// <summary>The order line this line answers.</summary>
        [JsonPropertyName("order_line_reference")]
        public MarminAeOrderLineReference? OrderLineReference { get; init; }

        /// <summary>The despatch line this line answers.</summary>
        [JsonPropertyName("despatch_line_reference")]
        public MarminAeDespatchLineReference? DespatchLineReference { get; init; }

        /// <summary>Another document this line refers to.</summary>
        [JsonPropertyName("document_reference")]
        public MarminAeDocumentReference? DocumentReference { get; init; }

        /// <summary>Amounts added to the line.</summary>
        [JsonPropertyName("charges")]
        public IReadOnlyList<MarminAeCharge>? Charges { get; init; }

        /// <summary>Amounts deducted from the line.</summary>
        [JsonPropertyName("allowances")]
        public IReadOnlyList<MarminAeAllowance>? Allowances { get; init; }

        /// <summary>The buyer's own identifier for the item.</summary>
        [JsonPropertyName("buyer_item_identification")]
        public MarminAeItemIdentification? BuyerItemIdentification { get; init; }

        /// <summary>The seller's own identifier for the item.</summary>
        [JsonPropertyName("seller_item_identification")]
        public MarminAeItemIdentification? SellerItemIdentification { get; init; }

        /// <summary>The item's identifier in a registered scheme.</summary>
        [JsonPropertyName("standard_item_identification")]
        public MarminAeStandardItemIdentification? StandardItemIdentification { get; init; }

        /// <summary>
        ///     Further item identifiers, which is where a service accounting code goes.
        /// </summary>
        [JsonPropertyName("additional_item_identification")]
        public IReadOnlyList<MarminAeAdditionalItemIdentification>? AdditionalItemIdentification { get; init; }

        /// <summary>Where the goods came from.</summary>
        [JsonPropertyName("origin_country")]
        public MarminAeOriginCountry? OriginCountry { get; init; }

        /// <summary>How the item is classified.</summary>
        [JsonPropertyName("commodity_classification")]
        public MarminAeCommodityClassification? CommodityClassification { get; init; }

        /// <summary>Attributes of the item.</summary>
        [JsonPropertyName("additional_item_property")]
        public IReadOnlyList<MarminAeAdditionalItemProperty>? AdditionalItemProperty { get; init; }

        /// <summary>The production batch the goods came from.</summary>
        [JsonPropertyName("lot_number_id")]
        public string? LotNumberId { get; init; }

        /// <summary>An identifier the integration carries on the line for its own purposes.</summary>
        [JsonPropertyName("line_object_identifier")]
        public string? LineObjectIdentifier { get; init; }
    }

    /// <summary>A supporting file sent with a document.</summary>
    /// <remarks>
    ///     Files ride inside the JSON body as Base64, which is why they count against the payload
    ///     cap at roughly four thirds of their size on disk. The vendor accepts a fixed list of
    ///     media types and rejects everything else with a field-level error.
    /// </remarks>
    public sealed record MarminAeAttachmentRequest
    {
        /// <summary>The file name, extension included.</summary>
        [JsonPropertyName("file_name")]
        public required string FileName { get; init; }

        /// <summary>The file's media type.</summary>
        [JsonPropertyName("file_type")]
        public required string FileType { get; init; }

        /// <summary>The file's bytes, Base64-encoded with no data-URI prefix.</summary>
        [JsonPropertyName("file_content")]
        public required string FileContent { get; init; }
    }
}

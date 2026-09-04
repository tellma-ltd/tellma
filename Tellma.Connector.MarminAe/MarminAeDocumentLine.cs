// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>One line of a document as the vendor holds it.</summary>
    /// <remarks>
    ///     The submitted content, plus the amounts the vendor computed for the line.
    /// </remarks>
    public sealed record MarminAeDocumentLine
    {
        /// <summary>What was sold.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>A fuller description of what was sold.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; init; }

        /// <summary>How much of it.</summary>
        [JsonPropertyName("quantity")]
        public decimal? Quantity { get; init; }

        /// <summary>The unit the quantity is counted in.</summary>
        [JsonPropertyName("unit_code")]
        public string? UnitCode { get; init; }

        /// <summary>What it cost per unit.</summary>
        [JsonPropertyName("price")]
        public MarminAePrice? Price { get; init; }

        /// <summary>How it is treated for VAT.</summary>
        [JsonPropertyName("classified_tax_category")]
        public MarminAeTaxCategory? ClassifiedTaxCategory { get; init; }

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

        /// <summary>Further item identifiers, including the service accounting code.</summary>
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

        /// <summary>The line total before tax.</summary>
        [JsonPropertyName("net_amount")]
        public decimal? NetAmount { get; init; }

        /// <summary>The line total before line charges and allowances.</summary>
        [JsonPropertyName("base_amount")]
        public decimal? BaseAmount { get; init; }

        /// <summary>The line total including tax.</summary>
        [JsonPropertyName("total_amount")]
        public decimal? TotalAmount { get; init; }

        /// <summary>The line total including tax, converted to the tax currency.</summary>
        [JsonPropertyName("total_amount_in_aed")]
        public decimal? TotalAmountInAed { get; init; }

        /// <summary>The VAT on the line.</summary>
        [JsonPropertyName("tax_amount")]
        public decimal? TaxAmount { get; init; }

        /// <summary>The VAT on the line, converted to the tax currency.</summary>
        [JsonPropertyName("tax_amount_in_aed")]
        public decimal? TaxAmountInAed { get; init; }
    }

    /// <summary>What a line charged per unit, as the vendor holds it.</summary>
    /// <remarks>
    ///     The unit price comes back under one of two names depending on the endpoint, so both are
    ///     read and <see cref="Amount" /> reports whichever arrived. Reading only one of them would
    ///     produce a null that looks like a free line.
    /// </remarks>
    public sealed record MarminAePrice
    {
        /// <summary>The unit price, under the name the document endpoints use.</summary>
        [JsonPropertyName("price_amount")]
        public decimal? PriceAmount { get; init; }

        /// <summary>The unit price, under the name the request used.</summary>
        [JsonPropertyName("base_amount")]
        public decimal? BaseAmount { get; init; }

        /// <summary>The unit price, from whichever field carried it.</summary>
        [JsonIgnore]
        public decimal? Amount => PriceAmount ?? BaseAmount;

        /// <summary>How many units the price covers.</summary>
        [JsonPropertyName("base_quantity")]
        public decimal? BaseQuantity { get; init; }

        /// <summary>A discount taken off the unit price.</summary>
        [JsonPropertyName("allowance")]
        public MarminAePriceAllowance? Allowance { get; init; }
    }
}

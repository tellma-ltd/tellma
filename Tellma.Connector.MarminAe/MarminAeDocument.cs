// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>A document as the vendor holds it, in any of the four families.</summary>
    /// <remarks>
    ///     <para>
    ///         One type reads all four families, because the vendor returns one shape for all four:
    ///         the fields a family does not use simply come back absent. Which family a document
    ///         belongs to is not on the wire at all — it is carried on <see cref="Kind" />, stamped
    ///         from the request that asked for it.
    ///     </para>
    ///     <para>
    ///         Every member is optional. A response model that insisted on a field would turn one
    ///         omission by the vendor into a failed read of the entire document, which is a much
    ///         worse outcome than a null where a value was expected.
    ///     </para>
    /// </remarks>
    public sealed record MarminAeDocument
    {
        /// <summary>Which family this document was read from.</summary>
        /// <remarks>
        ///     Not a wire field. The client stamps it from the request, since the response carries
        ///     no discriminator of its own and a caller holding a bare document would otherwise
        ///     have no way to route it back to the right endpoint.
        /// </remarks>
        [JsonIgnore]
        public MarminAeDocumentKind? Kind { get; init; }

        /// <summary>The vendor's identifier for the document, and the id every route takes.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        /// <summary>The organization the document belongs to.</summary>
        [JsonPropertyName("org_id")]
        public string? OrgId { get; init; }

        /// <summary>The document's position in the organization's sequence.</summary>
        [JsonPropertyName("document_sequence")]
        public long? DocumentSequence { get; init; }

        /// <summary>The document number as it appears on the document.</summary>
        [JsonPropertyName("document_number")]
        public string? DocumentNumber { get; init; }

        /// <summary>Which system the document originated in.</summary>
        [JsonPropertyName("document_source")]
        public string? DocumentSource { get; init; }

        /// <summary>The eight binary flags encoding which special supply scenarios applied.</summary>
        [JsonPropertyName("profile_execution_id")]
        public string? ProfileExecutionId { get; init; }

        /// <summary>The day the document was issued.</summary>
        [JsonPropertyName("issue_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? IssueDate { get; init; }

        /// <summary>The time of day the document was issued, in Gulf Standard Time.</summary>
        [JsonPropertyName("issue_time")]
        [JsonConverter(typeof(MarminAeTimeOnlyConverter))]
        public TimeOnly? IssueTime { get; init; }

        /// <summary>Which kind of invoice this is, on the two invoice families.</summary>
        [JsonPropertyName("invoice_type_code")]
        public string? InvoiceTypeCode { get; init; }

        /// <summary>Which kind of credit note this is, on the two credit-note families.</summary>
        [JsonPropertyName("credit_note_type_code")]
        public string? CreditNoteTypeCode { get; init; }

        /// <summary>
        ///     The document's type code, from whichever of the two family-specific fields carried
        ///     it.
        /// </summary>
        [JsonIgnore]
        public string? TypeCode => InvoiceTypeCode ?? CreditNoteTypeCode;

        /// <summary>Why a credit note was issued.</summary>
        [JsonPropertyName("discrepancy_response")]
        public string? DiscrepancyResponse { get; init; }

        /// <summary>A fuller explanation alongside the discrepancy code.</summary>
        [JsonPropertyName("reason")]
        public string? Reason { get; init; }

        /// <summary>The day payment falls due.</summary>
        [JsonPropertyName("due_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? DueDate { get; init; }

        /// <summary>The day the VAT liability arose.</summary>
        [JsonPropertyName("tax_point_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? TaxPointDate { get; init; }

        /// <summary>A note shown on the document.</summary>
        [JsonPropertyName("note")]
        public string? Note { get; init; }

        /// <summary>The currency every amount on the document is in.</summary>
        [JsonPropertyName("document_currency_code")]
        public string? DocumentCurrencyCode { get; init; }

        /// <summary>The currency tax is reported in.</summary>
        [JsonPropertyName("tax_currency_code")]
        public string? TaxCurrencyCode { get; init; }

        /// <summary>The rate converting the document currency into the tax currency.</summary>
        [JsonPropertyName("tax_exchange_rate")]
        public MarminAeTaxExchangeRate? TaxExchangeRate { get; init; }

        /// <summary>The cost centre the document is booked to.</summary>
        [JsonPropertyName("accounting_cost")]
        public string? AccountingCost { get; init; }

        /// <summary>The customer's own reference for the document.</summary>
        [JsonPropertyName("buyer_reference")]
        public string? BuyerReference { get; init; }

        /// <summary>The period the document covers.</summary>
        [JsonPropertyName("invoice_period")]
        public MarminAeInvoicePeriod? InvoicePeriod { get; init; }

        /// <summary>The purchase order behind the document.</summary>
        [JsonPropertyName("order_reference")]
        public MarminAeOrderReference? OrderReference { get; init; }

        /// <summary>The documents this one refers to, including the one a credit note credits.</summary>
        [JsonPropertyName("billing_reference")]
        public IReadOnlyList<MarminAeDocumentReference>? BillingReference { get; init; }

        /// <summary>The despatch document behind the document.</summary>
        [JsonPropertyName("despatch_document_reference")]
        public MarminAeDocumentReference? DespatchDocumentReference { get; init; }

        /// <summary>The receipt behind the document.</summary>
        [JsonPropertyName("receipt_document_reference")]
        public MarminAeDocumentReference? ReceiptDocumentReference { get; init; }

        /// <summary>The statement the document belongs to.</summary>
        [JsonPropertyName("statement_document_reference")]
        public MarminAeDocumentReference? StatementDocumentReference { get; init; }

        /// <summary>The document that originated this one.</summary>
        [JsonPropertyName("originator_document_reference")]
        public MarminAeDocumentReference? OriginatorDocumentReference { get; init; }

        /// <summary>The contract the document is issued under.</summary>
        [JsonPropertyName("contract_document_reference")]
        public MarminAeDocumentReference? ContractDocumentReference { get; init; }

        /// <summary>The project the document belongs to.</summary>
        [JsonPropertyName("project_reference")]
        public MarminAeProjectReference? ProjectReference { get; init; }

        /// <summary>
        ///     The supplier, which the vendor fills in from the business profile whatever the
        ///     submission said.
        /// </summary>
        [JsonPropertyName("accounting_supplier_party")]
        public MarminAeParty? AccountingSupplierParty { get; init; }

        /// <summary>The customer.</summary>
        [JsonPropertyName("accounting_customer_party")]
        public MarminAeParty? AccountingCustomerParty { get; init; }

        /// <summary>Who is to be paid, when that is not the supplier.</summary>
        [JsonPropertyName("payee_party")]
        public MarminAePayeeParty? PayeeParty { get; init; }

        /// <summary>The beneficiary a free-zone supply named.</summary>
        [JsonPropertyName("buyer_customer_party")]
        public MarminAePartyReference? BuyerCustomerParty { get; init; }

        /// <summary>The principal a disclosed agent billed on behalf of.</summary>
        [JsonPropertyName("seller_supplier_party")]
        public MarminAePartyReference? SellerSupplierParty { get; init; }

        /// <summary>Who represents the supplier for tax purposes.</summary>
        [JsonPropertyName("tax_representative_party")]
        public MarminAeTaxRepresentativeParty? TaxRepresentativeParty { get; init; }

        /// <summary>Where and when what the document covers was handed over.</summary>
        [JsonPropertyName("delivery")]
        public MarminAeDelivery? Delivery { get; init; }

        /// <summary>How the document is to be paid.</summary>
        [JsonPropertyName("payment_means")]
        public IReadOnlyList<MarminAePaymentMeans>? PaymentMeans { get; init; }

        /// <summary>The terms payment is due under.</summary>
        [JsonPropertyName("payment_terms")]
        public MarminAePaymentTerms? PaymentTerms { get; init; }

        /// <summary>Amounts added at document level.</summary>
        [JsonPropertyName("charges")]
        public IReadOnlyList<MarminAeCharge>? Charges { get; init; }

        /// <summary>Amounts deducted at document level.</summary>
        [JsonPropertyName("allowances")]
        public IReadOnlyList<MarminAeAllowance>? Allowances { get; init; }

        /// <summary>
        ///     The files held against the document, which on a document just created include the
        ///     rendering the vendor generates for it.
        /// </summary>
        [JsonPropertyName("attachments")]
        public IReadOnlyList<MarminAeAttachment>? Attachments { get; init; }

        /// <summary>The document's lines.</summary>
        [JsonPropertyName("document_lines")]
        public IReadOnlyList<MarminAeDocumentLine>? DocumentLines { get; init; }

        /// <summary>The VAT owed, grouped by category and rate.</summary>
        [JsonPropertyName("tax_breakdown")]
        public IReadOnlyList<MarminAeTaxBreakdownItem>? TaxBreakdown { get; init; }

        /// <summary>The total of the document-level allowances.</summary>
        [JsonPropertyName("allowance_total_amount")]
        public decimal? AllowanceTotalAmount { get; init; }

        /// <summary>The total of the document-level charges.</summary>
        [JsonPropertyName("charge_total_amount")]
        public decimal? ChargeTotalAmount { get; init; }

        /// <summary>The sum of the line amounts, before document charges, allowances and tax.</summary>
        [JsonPropertyName("line_extension_amount")]
        public decimal? LineExtensionAmount { get; init; }

        /// <summary>The amount tax is computed on.</summary>
        [JsonPropertyName("tax_exclusive_amount")]
        public decimal? TaxExclusiveAmount { get; init; }

        /// <summary>The total VAT, in the document currency.</summary>
        [JsonPropertyName("tax_amount")]
        public decimal? TaxAmount { get; init; }

        /// <summary>The total VAT, converted to the tax currency.</summary>
        [JsonPropertyName("tax_amount_in_aed")]
        public decimal? TaxAmountInAed { get; init; }

        /// <summary>The amount including tax.</summary>
        [JsonPropertyName("tax_inclusive_amount")]
        public decimal? TaxInclusiveAmount { get; init; }

        /// <summary>How much had already been paid.</summary>
        [JsonPropertyName("prepaid_amount")]
        public decimal? PrepaidAmount { get; init; }

        /// <summary>The rounding adjustment applied to the payable amount.</summary>
        [JsonPropertyName("payable_rounding_amount")]
        public decimal? PayableRoundingAmount { get; init; }

        /// <summary>What is actually owed.</summary>
        [JsonPropertyName("payable_amount")]
        public decimal? PayableAmount { get; init; }

        /// <summary>What is owed, converted to the tax currency.</summary>
        [JsonPropertyName("payable_amount_in_aed")]
        public decimal? PayableAmountInAed { get; init; }

        /// <summary>The total of the line-level allowances.</summary>
        [JsonPropertyName("total_item_allowances")]
        public decimal? TotalItemAllowances { get; init; }

        /// <summary>The total of the line-level charges.</summary>
        [JsonPropertyName("total_item_charges")]
        public decimal? TotalItemCharges { get; init; }

        /// <summary>How much of the document is taxable.</summary>
        [JsonPropertyName("total_taxable_amount")]
        public decimal? TotalTaxableAmount { get; init; }

        /// <summary>How much of the document is not.</summary>
        [JsonPropertyName("total_non_taxable_amount")]
        public decimal? TotalNonTaxableAmount { get; init; }

        /// <summary>Whether the document travels over the exchange network.</summary>
        [JsonPropertyName("is_phase2_document")]
        public bool? IsPhase2Document { get; init; }

        /// <summary>
        ///     Where the document stands in the vendor's own lifecycle, as distinct from how its
        ///     transmission went.
        /// </summary>
        /// <remarks>
        ///     Not in the vendor's published field list, but returned on every document and named
        ///     in at least one of its refusals, so it is read rather than discarded. The vocabulary
        ///     is not published either, so it stays a string.
        /// </remarks>
        [JsonPropertyName("document_status")]
        public string? DocumentStatus { get; init; }

        /// <summary>
        ///     The tax point date the vendor settled on, whether it was supplied or derived.
        /// </summary>
        /// <remarks>Also outside the published field list, and also returned on every document.</remarks>
        [JsonPropertyName("effective_tax_point_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? EffectiveTaxPointDate { get; init; }

        /// <summary>The day the supply took place, where the vendor records one.</summary>
        [JsonPropertyName("supply_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? SupplyDate { get; init; }

        /// <summary>Whatever the vendor is carrying for the integration's own purposes.</summary>
        /// <remarks>
        ///     Left as raw JSON: the vendor publishes neither a shape for it nor a way to set it
        ///     through this API, and discarding a field a future release starts using would be worse
        ///     than carrying one nobody reads.
        /// </remarks>
        [JsonPropertyName("custom_data")]
        public JsonElement? CustomData { get; init; }

        /// <summary>What the vendor knows about the document beyond its content.</summary>
        [JsonPropertyName("meta_info")]
        public MarminAeDocumentMetaInfo? MetaInfo { get; init; }
    }
}

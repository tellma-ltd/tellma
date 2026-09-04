// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.Json.Serialization;

namespace Tellma.Connector.MarminAe
{
    /// <summary>What a sales invoice and a sales credit note have in common.</summary>
    /// <remarks>
    ///     <para>
    ///         The same type submits a document and replaces one. Replacement is a whole-document
    ///         operation at the vendor — it accepts no partial patch and clears what a payload
    ///         omits — so sharing the type makes "send everything you would have sent on create" a
    ///         property of the type system rather than a rule someone has to remember.
    ///     </para>
    ///     <para>
    ///         Nothing the vendor computes or derives can be expressed here. The supplier party,
    ///         the identifiers, the sequence number, every total, and the transmission metadata are
    ///         absent by construction, so a payload cannot carry a value the vendor would silently
    ///         overwrite.
    ///     </para>
    /// </remarks>
    public abstract record MarminAeSalesDocumentRequest
    {
        /// <summary>The day the document was issued.</summary>
        [JsonPropertyName("issue_date")]
        public required DateOnly IssueDate { get; init; }

        /// <summary>
        ///     The eight binary flags encoding which special supply scenarios apply. Which flags to
        ///     set is tax knowledge that belongs above this client; it transmits what it is given.
        /// </summary>
        [JsonPropertyName("profile_execution_id")]
        public required string ProfileExecutionId { get; init; }

        /// <summary>The ISO 4217 currency every amount on the document is in.</summary>
        [JsonPropertyName("document_currency_code")]
        public required string DocumentCurrencyCode { get; init; }

        /// <summary>The customer the document is issued to.</summary>
        [JsonPropertyName("accounting_customer_party")]
        public required MarminAePartyRequest AccountingCustomerParty { get; init; }

        /// <summary>The document's lines. The vendor requires at least one.</summary>
        [JsonPropertyName("document_lines")]
        public required IReadOnlyList<MarminAeDocumentLineRequest> DocumentLines { get; init; }

        /// <summary>
        ///     The document number as it appears on the document. The vendor assigns one when the
        ///     organization has auto-numbering on and the field is omitted.
        /// </summary>
        [JsonPropertyName("document_number")]
        public string? DocumentNumber { get; init; }

        /// <summary>The day the VAT liability arose, when it differs from the issue date.</summary>
        [JsonPropertyName("tax_point_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? TaxPointDate { get; init; }

        /// <summary>
        ///     The currency tax is reported in, which the vendor requires whenever the document is
        ///     not in the local currency.
        /// </summary>
        [JsonPropertyName("tax_currency_code")]
        public string? TaxCurrencyCode { get; init; }

        /// <summary>The rate converting the document currency into the tax currency.</summary>
        [JsonPropertyName("tax_exchange_rate")]
        public MarminAeTaxExchangeRate? TaxExchangeRate { get; init; }

        /// <summary>The beneficiary a free-zone supply names.</summary>
        [JsonPropertyName("buyer_customer_party")]
        public MarminAePartyReference? BuyerCustomerParty { get; init; }

        /// <summary>The principal a disclosed agent bills on behalf of.</summary>
        [JsonPropertyName("seller_supplier_party")]
        public MarminAePartyReference? SellerSupplierParty { get; init; }

        /// <summary>Where and when what the document covers was handed over.</summary>
        [JsonPropertyName("delivery")]
        public MarminAeDelivery? Delivery { get; init; }

        /// <summary>The period the document covers.</summary>
        [JsonPropertyName("invoice_period")]
        public MarminAeInvoicePeriod? InvoicePeriod { get; init; }

        /// <summary>
        ///     The time of day the document was issued, in Gulf Standard Time. The vendor treats an
        ///     omitted time as midnight.
        /// </summary>
        [JsonPropertyName("issue_time")]
        [JsonConverter(typeof(MarminAeTimeOnlyConverter))]
        public TimeOnly? IssueTime { get; init; }

        /// <summary>The cost centre the document is booked to.</summary>
        [JsonPropertyName("accounting_cost")]
        public string? AccountingCost { get; init; }

        /// <summary>The customer's own reference for the document.</summary>
        [JsonPropertyName("buyer_reference")]
        public string? BuyerReference { get; init; }

        /// <summary>A note shown on the document.</summary>
        [JsonPropertyName("note")]
        public string? Note { get; init; }

        /// <summary>Who is to be paid, when that is not the supplier.</summary>
        [JsonPropertyName("payee_party")]
        public MarminAePayeeParty? PayeeParty { get; init; }

        /// <summary>Who represents the supplier for tax purposes.</summary>
        [JsonPropertyName("tax_representative_party")]
        public MarminAeTaxRepresentativeParty? TaxRepresentativeParty { get; init; }

        /// <summary>The purchase order behind the document.</summary>
        [JsonPropertyName("order_reference")]
        public MarminAeOrderReference? OrderReference { get; init; }

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
        ///     How the document is to be paid. The vendor requires at least one instruction on
        ///     every document except a deemed supply.
        /// </summary>
        [JsonPropertyName("payment_means")]
        public IReadOnlyList<MarminAePaymentMeans>? PaymentMeans { get; init; }

        /// <summary>Amounts added at document level.</summary>
        [JsonPropertyName("charges")]
        public IReadOnlyList<MarminAeCharge>? Charges { get; init; }

        /// <summary>Amounts deducted at document level.</summary>
        [JsonPropertyName("allowances")]
        public IReadOnlyList<MarminAeAllowance>? Allowances { get; init; }

        /// <summary>The terms payment is due under.</summary>
        [JsonPropertyName("payment_terms")]
        public MarminAePaymentTerms? PaymentTerms { get; init; }

        /// <summary>How much has already been paid.</summary>
        [JsonPropertyName("prepaid_amount")]
        public decimal? PrepaidAmount { get; init; }

        /// <summary>The rounding adjustment applied to the payable amount.</summary>
        [JsonPropertyName("payable_rounding_amount")]
        public decimal? PayableRoundingAmount { get; init; }

        /// <summary>
        ///     Which system the document originated in. The vendor records the API itself when this
        ///     is omitted.
        /// </summary>
        [JsonPropertyName("document_source")]
        public string? DocumentSource { get; init; }

        /// <summary>Supporting files, which count against the payload cap once encoded.</summary>
        [JsonPropertyName("attachments")]
        public IReadOnlyList<MarminAeAttachmentRequest>? Attachments { get; init; }
    }

    /// <summary>A sales invoice, submitted or replaced.</summary>
    public sealed record MarminAeSalesInvoiceRequest : MarminAeSalesDocumentRequest
    {
        /// <summary>
        ///     Which kind of invoice this is. The vendor accepts a commercial invoice and an
        ///     out-of-scope one, and which applies depends on the supplier's VAT position.
        /// </summary>
        [JsonPropertyName("invoice_type_code")]
        public required string InvoiceTypeCode { get; init; }

        /// <summary>The day payment falls due.</summary>
        [JsonPropertyName("due_date")]
        public required DateOnly DueDate { get; init; }

        /// <summary>Other billing documents this invoice refers to.</summary>
        [JsonPropertyName("billing_reference")]
        public IReadOnlyList<MarminAeDocumentReference>? BillingReference { get; init; }
    }

    /// <summary>A sales credit note, submitted or replaced.</summary>
    /// <remarks>
    ///     A credit note adjusts an invoice already issued, so the vendor requires it to name the
    ///     invoice and to say why the adjustment is being made. Those two obligations, and the
    ///     credit-note type code, are the whole difference from an invoice.
    /// </remarks>
    public sealed record MarminAeSalesCreditNoteRequest : MarminAeSalesDocumentRequest
    {
        /// <summary>Which kind of credit note this is.</summary>
        [JsonPropertyName("credit_note_type_code")]
        public required string CreditNoteTypeCode { get; init; }

        /// <summary>
        ///     Why the credit note is being issued, as one of the authority's discrepancy codes.
        /// </summary>
        [JsonPropertyName("discrepancy_response")]
        public required string DiscrepancyResponse { get; init; }

        /// <summary>
        ///     The documents being credited. The vendor requires at least one, naming the original
        ///     invoice.
        /// </summary>
        [JsonPropertyName("billing_reference")]
        public required IReadOnlyList<MarminAeDocumentReference> BillingReference { get; init; }

        /// <summary>A fuller explanation alongside the discrepancy code.</summary>
        [JsonPropertyName("reason")]
        public string? Reason { get; init; }

        /// <summary>
        ///     The day payment falls due. Optional here, unlike on an invoice: the vendor does not
        ///     list it among a credit note's fields, but does accept it, so it is offered rather
        ///     than obliged.
        /// </summary>
        [JsonPropertyName("due_date")]
        [JsonConverter(typeof(MarminAeDateOnlyConverter))]
        public DateOnly? DueDate { get; init; }
    }
}

using System;
using System.Collections.Generic;

namespace Tellma.Repository.Application
{
    /// <summary>
    /// One Tellma document, shaped for submission to the Marmin UAE e-invoicing API.
    /// Produced by <c>[dal].[MarminAe__GetInvoices]</c>.
    /// </summary>
    /// <remarks>
    /// Far smaller than <see cref="ZatcaInvoice"/> on purpose: Marmin derives the supplier party,
    /// the document identifiers and every total server-side, so there is nothing here that the
    /// vendor will also compute. The one number Tellma still owns is each line's unit price.
    /// </remarks>
    public class MarminAeInvoice
    {
        /// <summary>The Tellma document Id.</summary>
        public int Id { get; set; }

        /// <summary>
        /// Which vendor endpoint to submit to. Matches the <c>MarminAeDocumentKind</c> enum member
        /// names exactly (<c>SalesInvoice</c>, <c>SalesCreditNote</c>).
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>The literal <c>invoice_type_code</c> or <c>credit_note_type_code</c>.</summary>
        public string TypeCode { get; set; }

        /// <summary>
        /// Tellma's own document code, sent as <c>document_number</c>. Vendor auto-numbering must
        /// be off. It is also the key the resubmit path uses to ask the vendor whether a
        /// submission that appeared to fail actually landed.
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>The accounting date, not the moment of closing.</summary>
        public DateTime IssueDate { get; set; }

        /// <summary>Required on an invoice; optional on a credit note.</summary>
        public DateTime? DueDate { get; set; }

        /// <summary>The eight supply-scenario flags (the UAE analogue of ZATCA's ITT code).</summary>
        public string ProfileExecutionId { get; set; }

        /// <summary>ISO 4217.</summary>
        public string DocumentCurrencyCode { get; set; }

        public string Note { get; set; }

        public string BuyerReference { get; set; }

        /// <summary>Credit notes only: the authority's discrepancy code. Vendor-required.</summary>
        public string DiscrepancyResponse { get; set; }

        /// <summary>Credit notes only: the human-readable reason.</summary>
        public string Reason { get; set; }

        #region Customer

        public string CustomerName { get; set; }

        /// <summary>Vendor-required. Validated at close, because it is often blank in Tellma.</summary>
        public string CustomerEmail { get; set; }

        /// <summary>The Peppol routing address: the customer's tax registration number.</summary>
        public string CustomerEndpointId { get; set; }

        /// <summary>Tenant-wide, from General Settings.</summary>
        public string CustomerEndpointSchemeId { get; set; }

        /// <summary>Only populated for a UAE customer; the vendor rejects it on a foreign party.</summary>
        public string CustomerTin { get; set; }

        public string CustomerStreetName { get; set; }
        public string CustomerAdditionalStreetName { get; set; }
        public string CustomerCityName { get; set; }
        public string CustomerPostalZone { get; set; }

        /// <summary>Must be an emirate code, not a name. Validated at close.</summary>
        public string CustomerCountrySubentity { get; set; }

        public string CustomerCountry { get; set; }
        public string CustomerCountryCode { get; set; }

        #endregion

        public string PaymentMeansCode { get; set; }

        public string PayeeFinancialAccountId { get; set; }

        public decimal? PayableRoundingAmount { get; set; }

        /// <summary>Credit notes only: the code of the invoice being adjusted.</summary>
        public string BillingReferenceId { get; set; }

        /// <summary>Credit notes only: the issue date of the invoice being adjusted.</summary>
        public DateTime? BillingReferenceIssueDate { get; set; }

        /// <summary>The invoice lines, in document order.</summary>
        public List<MarminAeInvoiceLine> Lines { get; set; } = new();
    }

    /// <summary>One line of a <see cref="MarminAeInvoice"/>.</summary>
    public class MarminAeInvoiceLine
    {
        public int LineNumber { get; set; }

        public string Name { get; set; }

        /// <summary>Vendor-required; falls back to <see cref="Name"/> in SQL.</summary>
        public string Description { get; set; }

        public decimal Quantity { get; set; }

        /// <summary>UN/ECE Recommendation 20. Free text in Tellma, so validated at close.</summary>
        public string UnitCode { get; set; }

        /// <summary>The net unit price.</summary>
        public decimal PriceBaseAmount { get; set; }

        public decimal PriceBaseQuantity { get; set; }

        /// <summary>UNCL5305 letter: S, Z, E or O.</summary>
        public string TaxCategoryId { get; set; }

        /// <summary>
        /// A percentage (5 for 5%), not the 0..1 fraction ZATCA uses and that Tellma stores in
        /// <c>Resources.VatRate</c>. The SQL multiplies by 100.
        /// </summary>
        public decimal TaxPercent { get; set; }

        /// <summary>Vendor-required when <see cref="TaxCategoryId"/> is E (exempt).</summary>
        public string TaxExemptionReasonCode { get; set; }

        /// <summary>Vendor-required when <see cref="TaxCategoryId"/> is E (exempt).</summary>
        public string TaxExemptionReason { get; set; }

        public string SellerItemIdentification { get; set; }

        public string StandardItemIdentification { get; set; }
    }
}

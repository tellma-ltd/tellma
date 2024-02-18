using System;
using System.Collections.Generic;
using Tellma.Repository.Common;

namespace Tellma.Repository.Application
{
    /// <summary>
    /// Output of closing documents, including a mapping of the closed documents to e-invoices for ZATCA clearance. <br/>
    /// This object contains: <br/>
    ///  - Errors. <br/>
    ///  - InboxStatuses. <br/>
    ///  - ZATCA invoices. <br/>
    /// </summary>
    public class CloseDocumentOutput : InboxStatusOutput
    {
        public CloseDocumentOutput(
            IEnumerable<ZatcaInvoice> invoices, 
            int previousCounterValue, 
            string previousInvoiceHash, 
            IEnumerable<ValidationError> errors, 
            IEnumerable<InboxStatus> inboxStatuses) : base(errors, inboxStatuses)
        {
            Invoices = invoices;
            PreviousCounterValue = previousCounterValue;
            PreviousInvoiceHash = previousInvoiceHash;
        }

        public IEnumerable<ZatcaInvoice> Invoices { get; }
        public int PreviousCounterValue { get; }
        public string PreviousInvoiceHash { get; }
    }

    public class ZatcaInvoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid UniqueInvoiceIdentifier { get; set; }
        public DateTimeOffset InvoiceIssueDateTime { get; set; }
        public int InvoiceType { get; set; }
        public bool IsSimplified { get; set; }
        public bool IsThirdParty { get; set; }
        public bool IsNominal { get; set; }
        public bool IsExports { get; set; }
        public bool IsSummary { get; set; }
        public bool IsSelfBilled { get; set; }
        public string InvoiceNote { get; set; }
        public string InvoiceCurrency { get; set; }
        public string PurchaseOrderId { get; set; }
        public string BillingReferenceId { get; set; }
        public string ContractId { get; set; }
        public string BuyerId { get; set; }
        public string BuyerIdScheme { get; set; } // VAT or else
        public string BuyerAddressStreet { get; set; }
        public string BuyerAddressAdditionalStreet { get; set; }
        public string BuyerAddressBuildingNumber { get; set; }
        public string BuyerAddressAdditionalNumber { get; set; }
        public string BuyerAddressCity { get; set; }
        public string BuyerAddressPostalCode { get; set; }
        public string BuyerAddressProvince { get; set; }
        public string BuyerAddressDistrict { get; set; }
        public string BuyerAddressCountryCode { get; set; }
        public string BuyerName { get; set; }
        public DateTime SupplyDate { get; set; }
        public DateTime SupplyEndDate { get; set; }
        public int PaymentMeans { get; set; }
        public string ReasonForIssuanceOfCreditDebitNote { get; set; }
        public string PaymentTerms { get; set; }
        public string PaymentAccountId { get; set; }
        public decimal InvoiceTotalVatAmountInAccountingCurrency { get; set; }
        public string AccountingCurrency { get; set; }
        public decimal PrepaidAmount { get; set; }
        public decimal RoundingAmount { get; set; }
        public List<ZatcaAllowanceCharge> AllowanceCharges { get; } = new();
        public List<ZatcaInvoiceLine> Lines { get; } = new();
    }

    public class ZatcaAllowanceCharge
    {
        public bool IsCharge { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public string ReasonCode { get; set; }
        public string VatCategory { get; set; } // E, S, Z, O
        public decimal VatRate { get; set; }
    }

    public class ZatcaInvoiceLine
    {
        public int Id { get; set; }
        public string PrepaymentId { get; set; }
        public Guid PrepaymentUuid { get; set; }
        public DateTimeOffset PrepaymentIssueDateTime { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityUnit { get; set; }
        public decimal NetAmount { get; set; }
        public bool AllowanceChargeIsCharge { get; set; }
        public decimal AllowanceChargeAmount { get; set; }
        public string AllowanceChargeReason { get; set; }
        public string AllowanceChargeReasonCode { get; set; }
        public decimal VatAmount { get; set; }
        public decimal PrepaymentVatCategoryTaxableAmount { get; set; }
        public string ItemName { get; set; }
        public string ItemBuyerIdentifier { get; set; }
        public string ItemSellerIdentifier { get; set; }
        public string ItemStandardIdentifier { get; set; }
        public decimal ItemNetPrice { get; set; }
        public string ItemVatCategory { get; set; } // E, S, Z, O
        public decimal ItemVatRate { get; set; }
        public string ItemVatExemptionReasonCode { get; set; }
        public string ItemVatExemptionReasonText { get; set; }
        public string PrepaymentVatCategory { get; set; } // E, S, Z, O
        public decimal PrepaymentVatRate { get; set; }
        public decimal ItemPriceBaseQuantity { get; set; }
        public string ItemPriceBaseQuantityUnit { get; set; }
        public decimal ItemPriceDiscount { get; set; }
        public decimal ItemGrossPrice { get; set; }
    }
}


namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Model for the ZATCA e-invoice, as specified in the 
    /// <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_EInvoice_Data_Dictionary%20vF.xlsx">E-Invoice Data Dictionary (2023-05-19)</see>.
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// <b>BT-1</b> 
        /// <br/> 
        /// A unique identification of the Invoice.
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// <b>KSA-1</b> 
        /// <br/> 
        /// Globally unique reference identifying the invoice.
        /// </summary>
        public Guid UniqueInvoiceIdentifier { get; set; }

        /// <summary>
        /// <b>BT-2</b> + <b>KSA-25</b> 
        /// <br/> 
        /// The date/time when the Invoice was issued  as per Article 53 of the VAT Implementing Regulation.
        /// </summary>
        public DateTimeOffset InvoiceIssueDateTime { get; set; }

        /// <summary>
        /// <b>BT-3</b> 
        /// <br/> 
        /// The functional type of the Invoice.
        /// </summary>
        public InvoiceType InvoiceType { get; set; }

        /// <summary>
        /// <b>KSA-2</b> 
        /// <br/> 
        /// The invoice subtype and invoices transactions.
        /// </summary>
        public InvoiceTransaction InvoiceTypeTransactions { get; set; } // Array of bits

        /// <summary>
        /// <b>BT-22</b> 
        /// <br/> 
        /// Textual notes that give unstructured information that is relevant to the Invoice as a whole.
        /// </summary>
        public List<string> InvoiceNotes { get; set; } = new();

        /// <summary>
        /// <b>BT-5</b> 
        /// <br/> 
        /// The currency in which all Invoice amounts are given, except for the <see cref="InvoiceTotalVatAmountInAccountingCurrency"/>.
        /// </summary>
        public string? InvoiceCurrency { get; set; }

        /// <summary>
        /// <b>BT-6</b> 
        /// <br/> 
        /// The currency used for VAT accounting and reporting purposes as accepted or required in the country of the Seller.
        /// <br/>
        /// Shall be used in combination with the <see cref="InvoiceTotalVatAmountInAccountingCurrency"/> (BT-111).
        /// </summary>
        public string TaxCurrency => "SAR";

        /// <summary>
        /// <b>BT-13</b> 
        /// <br/> 
        /// An identifier of a referenced purchase order, issued by the Buyer.
        /// </summary>
        public string? PurchaseOrderId { get; set; }

        /// <summary>
        /// <b>BT-25</b> 
        /// <br/> 
        /// The sequential number (<see cref="InvoiceNumber"/> BT-1) of the original invoice(s) that the credit/debit note is related to.
        /// </summary>
        public string? BillingReferenceId { get; set; }

        /// <summary>
        /// <b>BT-12</b> 
        /// <br/> 
        /// The identification of a contract.
        /// </summary>
        public string? ContractId { get; set; }

        // ??? What is the "Scope" of the counter? Document type (standard vs simplified) + (tax invoice, debit note, credit note) + Tenant?
        /// <summary>
        /// <b>KSA-16</b> 
        /// <br/> 
        /// Invoice counter value.
        /// </summary>
        public string? InvoiceCounterValue { get; set; }

        /// <summary>
        /// <b>KSA-13</b> 
        /// <br/> 
        /// The base64 encoded SHA256 hash of the previous invoice.  <br/>
        /// This hash will be computed from the business payload of the previous invoice: UBL XML of the previous invoice without tags for QR code(KSA-14) and cryptographic stamp (KSA-15). <br/>
        /// For the first invoice, the previous invoice hash is "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==", the equivalent for base64 encoded SHA256 of "0" (zero) character.
        /// <para/>
        /// More details can be found in the <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_ZATCA_Electronic_Invoice_Security_Features_Implementation_Standards_vF.pdf">Security Features Implementation Standard</see>.  <br/>
        /// </summary>
        public string? PreviousInvoiceHash { get; set; }

        /// <summary>
        /// The seller.
        /// </summary>
        public Party? Seller { get; set; }

        /// <summary>
        /// The buyer.
        /// </summary>
        public Party? Buyer { get; set; }

        /// <summary>
        /// <b>KSA-5</b> 
        /// <br/> 
        /// The date when the supply is performed. <br/>
        /// For credit and debit notes , it acts as the original supply date.
        /// </summary>
        public DateTime SupplyDate { get; set; }

        /// <summary>
        /// <b>KSA-24</b> 
        /// <br/> 
        /// Calendar field "End Date" for Continuous Supplies.
        /// </summary>
        public DateTime SupplyEndDate { get; set; }

        /// <summary>
        /// <b>BT-81</b> 
        /// <br/> 
        /// The means, expressed as code, for how a payment is expected to be or has been settled. <br/>
        /// Entries are subset from the <see href="https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred4461.htm">UNTDID 4461 code list</see>.
        /// </summary>
        public PaymentMeans PaymentMeans { get; set; }

        /// <summary>
        /// <b>KSA-10</b> 
        /// <br/> 
        /// Reasons for issuance of credit/debit note as per Article 40 (paragraph 1) and Article 54 (3) of KSA VAT regulations, a Credit and Debit Note is issued for these 5 instances: <br/>
        /// - Cancellation or suspension of the supplies after its occurrence either wholly or partially <br/>
        /// - In case of essential change or amendment in the supply, which leads to the change of the VAT due <br/>
        /// - Amendment of the supply value which is pre-agreed upon between the supplier and consumer <br/>
        /// - In case of goods or services refund. <br/>
        /// - In case of change in Seller's or Buyer's information. <br/>
        /// </summary>
        public List<string> ReasonsForIssuanceOfCreditDebitNote { get; set; } = new();

        /// <summary>
        /// <b>KSA-22</b> 
        /// <br/> 
        /// The payment terms, if mode of payment is credit. Free text
        /// </summary>
        public string? PaymentTerms { get; set; }

        /// <summary>
        /// <b>BT-84</b> 
        /// <br/> 
        /// The account number, IBAN, to which the transfer should be made.
        /// In the case of factoring this account is owned by the factor.
        /// </summary>
        public string? PaymentAccountId { get; set; }

        /// <summary>
        /// Document-level Allowance/Charge.
        /// </summary>
        public List<AllowanceCharge> AllowanceCharges { get; set; } = new();

        // Auto computed fields?

        /// <summary>
        /// <b>BT-106</b> 
        /// <br/> 
        /// Auto-calculated as the sum of all <see cref="InvoiceLine.NetAmount"/> (amount without VAT).
        /// </summary>
        public decimal SumOfInvoiceLineNetAmount => Lines.Sum(e => e.NetAmount); // Rule BR-CO-10

        /// <summary>
        /// <b>BT-107</b> 
        /// <br/> 
        /// Sum of all allowances on document level in the Invoice. <br/>
        /// Allowances on line level are included in the Invoice line net amount which is summed up into the <see cref="SumOfInvoiceLineNetAmount"/>. <br/>
        /// Auto-calculated as <see cref="AllowanceCharge.Amount"/> if it's an allowance.
        /// </summary>
        public decimal SumOfAllowancesOnDocumentLevel => AllowanceCharges?.Where(e => e.Indicator == AllowanceChargeType.Allowance)?.Sum(e => e.Amount) ?? 0m;// Rule BR-CO-11

        /// <summary>
        /// <b>BT-108</b> 
        /// <br/> 
        /// Sum of all charges on document level in the Invoice. <br/>
        /// Charges on line level are included in the Invoice line net amount which is summed up into the <see cref="SumOfInvoiceLineNetAmount"/>. <br/>
        /// Auto-calculated as <see cref="AllowanceCharge.Amount"/> if it's a charge.
        /// </summary>
        public decimal SumOfChargesDocumentLevel => AllowanceCharges?.Where(e => e.Indicator == AllowanceChargeType.Charge)?.Sum(e => e.Amount) ?? 0m; // Rule BR-CO-12

        /// <summary>
        /// <b>BT-109</b> 
        /// <br/> 
        /// The total amount of the Invoice without VAT. <br/>
        /// Auto-calculated as <see cref="SumOfInvoiceLineNetAmount"/> - <see cref="SumOfAllowancesOnDocumentLevel"/> + <see cref="SumOfChargesDocumentLevel"/>.
        /// </summary>
        public decimal InvoiceTotalAmountWithoutVat => SumOfInvoiceLineNetAmount - SumOfAllowancesOnDocumentLevel + SumOfChargesDocumentLevel; // Rule BR-CO-13

        /// <summary>
        /// <b>BT-110</b> 
        /// <br/> 
        /// The total VAT amount for the Invoice. <br/>
        /// Auto-calculated as <see cref="VatCategoryTaxAmount"/>.
        /// </summary>
        public decimal InvoiceTotalVatAmount => VatCategoryTaxAmount; // Rule BR-CO-14

        /// <summary>
        /// <b>BT-111</b> 
        /// <br/> 
        /// The VAT total amount expressed in the accounting currency accepted or required in the country of the Seller. <br/>
        /// To be used when the VAT accounting currency (BT-6) differs from the Invoice currency code (BT-5). <br/>
        /// The VAT amount in accounting currency is not used in the calculation of the Invoice totals.
        /// </summary>
        public decimal InvoiceTotalVatAmountInAccountingCurrency { get; set; }

        /// <summary>
        /// <b>BT-112</b> 
        /// <br/> 
        /// The total amount of the Invoice with VAT. <br/>
        /// Auto-calculated as <see cref="InvoiceTotalAmountWithoutVat"/> + <see cref="InvoiceTotalVatAmount"/>.
        /// </summary>
        public decimal InvoiceTotalAmountWithVat => InvoiceTotalAmountWithoutVat + InvoiceTotalVatAmount; // Rule BR-CO-15

        /// <summary>
        /// <b>BT-113</b> 
        /// <br/> 
        /// The sum of amounts which have been paid in advance including VAT.
        /// This amount is subtracted from the <see cref="InvoiceTotalAmountWithVat"/> to calculate the <see cref="AmountDueForPayment"/>.
        /// </summary>
        public decimal PrepaidAmount { get; set; }

        /// <summary>
        /// <b>BT-114</b> 
        /// <br/> 
        /// Amount which must be added to total in order to round off the payment amount.
        /// </summary>
        public decimal RoundingAmount { get; set; }

        /// <summary>
        /// <b>BT-115</b> 
        /// <br/> 
        /// The outstanding amount that is requested to be paid. <br/>
        /// Auto-computed as <see cref="InvoiceTotalAmountWithVat"/> - <see cref="PrepaidAmount"/> + <see cref="RoundingAmount"/>. <br/> 
        /// The amount is zero in case of a fully paid Invoice.
        /// </summary>
        public decimal AmountDueForPayment => InvoiceTotalAmountWithVat - PrepaidAmount + RoundingAmount; // Rule BR-CO-16

        /// <summary>
        /// <b>BT-116</b> 
        /// <br/> 
        /// Sum of all taxable amounts subject to a specific VAT category code and VAT category rate (if the VAT category rate is applicable). <br/>
        /// The sum of <see cref="InvoiceLine.NetAmount"/> minus allowances on document level which are subject to a specific VAT category code and VAT category rate (if the VAT category rate is applicable).
        /// </summary>
        public decimal VatCategoryTaxableAmount { get; set; }

        /// <summary>
        /// <b>BT-117</b> 
        /// <br/> 
        /// Auto-computed as <see cref="VatCategoryTaxableAmount"/> x <see cref="VatRate"/> rounded to 2 decimal places.
        /// </summary>
        public decimal VatCategoryTaxAmount => decimal.Round(VatCategoryTaxableAmount * VatRate, 2); // Rule BR-CO-17

        /// <summary>
        /// <b>BT-118</b> 
        /// <br/> 
        /// Coded identification of a VAT category.
        /// </summary>
        public VatCategory VatCategory { get; set; }

        /// <summary>
        /// <b>BT-119</b> 
        /// <br/> 
        /// The VAT rate, represented as percentage that applies for the relevant VAT category. <br/>
        /// A rate between 0.0000 and 1.0000
        /// </summary>
        public decimal VatRate { get; set; }

        /// <summary>
        /// <b>BT-120</b> 
        /// <br/> 
        /// A textual statement of the reason why the amount is exempted from VAT or why no VAT is being charged.
        /// </summary>
        public string? VatExemptionReason { get; set; }

        /// <summary>
        /// <b>BT-121</b> 
        /// <br/> 
        /// A coded statement of the reason for why the amount is exempted from VAT.
        /// </summary>
        public string? VatExemptionReasonCode { get; set; }

        /// <summary>
        /// Line items.
        /// </summary>
        public List<InvoiceLine> Lines { get; set; } = new();
    }
}

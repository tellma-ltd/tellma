namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Model for the ZATCA e-invoice VAT Breakdown, as specified in the 
    /// <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_EInvoice_Data_Dictionary%20vF.xlsx">E-Invoice Data Dictionary (2023-05-19)</see>.
    /// </summary>
    public class VatBreakdownEntry
    {
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
        public decimal VatCategoryTaxAmount { get; set; } // => decimal.Round(VatCategoryTaxableAmount * VatRate, 2); // Rule BR-CO-17

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
        public string? VatExemptionReasonText { get; set; }

        /// <summary>
        /// <b>BT-121</b> 
        /// <br/> 
        /// A coded statement of the reason for why the amount is exempted from VAT.
        /// </summary>
        public VatExemptionReason? VatExemptionReasonCode { get; set; }
    }
}

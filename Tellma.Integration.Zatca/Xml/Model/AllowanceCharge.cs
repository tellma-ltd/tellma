namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Model for the document level allowance/charge, as specified in the
    /// <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_EInvoice_Data_Dictionary%20vF.xlsx">E-Invoice Data Dictionary (2023-05-19)</see>.
    /// </summary>
    public class AllowanceCharge
    {
        /// <summary>
        /// Indicates whether this Allowance/Charge describes an allowance or a charge.
        /// </summary>
        public AllowanceChargeType Indicator { get; set; }

        /// <summary>
        /// <b>BT-94</b> (Allowance), <b>BT-101</b> (Charge)
        /// <br/> 
        /// The percentage that may be used, in conjunction with <see cref="BaseAmount"/>, to calculate the <see cref="Amount"/>. <br/>
        /// Must be a rate between 0.0000 and 1.0000
        /// <para/>
        /// Note: You only have to specify the <see cref="Amount"/>. This is optional.
        /// </summary>
        public decimal Percentage { get; set; }

        /// <summary>
        /// <b>BT-92</b> (Allowance), <b>BT-99</b> (Charge)
        /// <br/> 
        /// The amount of an allowance, without VAT.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// <b>BT-93</b> (Allowance), <b>BT-100</b> (Charge)
        /// <br/> 
        /// The base amount that may be used, in conjunction with the <see cref="Percentage"/>, to calculate the <see cref="Amount"/>.
        /// <para/>
        /// Note: You only have to specify the <see cref="Amount"/>. This is optional.
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// <b>BT-97</b> (Allowance), <b>BT-104</b> (Charge)
        /// <br/> 
        /// The reason for this Allowance/Charge.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// <b>BT-98</b> (Allowance), <b>BT-105</b> (Charge)
        /// <br/> 
        /// The reason code corresponding to the <see cref="Reason"/>. <br/>
        /// Entries from the <see href="https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred5189.htm">UNTDID 5189 code list</see> for discounts, 
        /// and from the <see href="https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred7161.htm">UNTDID 7161 code list</see> for charges.
        /// </summary>
        public string? ReasonCode { get; set; }

        /// <summary>
        /// <b>BT-95</b> (Allowance), <b>BT-102</b> (Charge)
        /// <br/> 
        /// A coded identification of what VAT category applies to the <see cref="AllowanceCharge"/>.
        /// </summary>
        public VatCategory VatCategory { get; set; }

        /// <summary>
        /// <b>BT-96</b> (Allowance), <b>BT-103</b> (Charge)
        /// <br/> 
        /// The VAT rate, represented as percentage that applies to the <see cref="AllowanceCharge"/>. <br/>
        /// Must be a rate between 0.0000 and 1.0000
        /// </summary>
        public decimal VatRate { get; set; }
    }
}

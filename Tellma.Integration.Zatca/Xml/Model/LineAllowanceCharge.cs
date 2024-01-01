namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Model for the line level allowance/charge, as specified in the
    /// <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_EInvoice_Data_Dictionary%20vF.xlsx">E-Invoice Data Dictionary (2023-05-19)</see>.
    /// </summary>
    public class LineAllowanceCharge
    {
        /// <summary>
        /// Indicates whether this Allowance/Charge describes an allowance or a charge.
        /// </summary>
        public AllowanceChargeType Indicator { get; set; }

        /// <summary>
        /// <b>BT-138</b> 
        /// <br/> 
        /// The percentage that may be used, in conjunction with <see cref="BaseAmount"/>, to calculate the <see cref="Amount"/>. <br/>
        /// Must be a rate between 0.0000 and 1.0000
        /// <para/>
        /// Note: You only have to specify the <see cref="Amount"/>. This is optional.
        /// </summary>
        public decimal Percentage { get; set; }

        /// <summary>
        /// <b>BT-136</b> 
        /// <br/> 
        /// The amount of an allowance, without VAT.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// <b>BT-137</b> 
        /// <br/> 
        /// The base amount that may be used, in conjunction with the <see cref="Percentage"/>, to calculate the <see cref="Amount"/>.
        /// <para/>
        /// Note: You only have to specify the <see cref="Amount"/>. This is optional.
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// <b>BT-139</b> 
        /// <br/> 
        /// The reason for allowance on line item-level.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// <b>BT-140</b> 
        /// <br/> 
        /// The reason code corresponding to the <see cref="Reason"/> for allowance on line item-level. <br/>
        /// Entries from the <see href="https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred5189.htm">UNTDID 5189 code list</see> for discounts, 
        /// and from the <see href="https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred7161.htm">UNTDID 7161 code list</see> for charges.
        /// </summary>
        public string? ReasonCode { get; set; }
    }
}

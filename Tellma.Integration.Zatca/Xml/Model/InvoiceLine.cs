using System.Text.RegularExpressions;

namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Model for a line in the ZATCA e-invoice, as specified in the 
    /// <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_EInvoice_Data_Dictionary%20vF.xlsx">E-Invoice Data Dictionary (2023-05-19)</see>.
    /// </summary>
    public class InvoiceLine
    {
        /// <summary>
        /// <b>BT-126</b> 
        /// <br/> 
        /// A unique identifier for the individual line within the Invoice. 
        /// This value should be only numeric value between 1 and 999,999.
        /// </summary>
        public int Identifier { get; set; }

        /// <summary>
        /// <b>KSA-26</b> 
        /// <br/> 
        /// The <see cref="Invoice.InvoiceNumber"/> (BT-1) of the associated Prepayment invoice(s).
        /// </summary>
        public string? PrepaymentId { get; set; }

        /// <summary>
        /// <b>KSA-27</b>
        /// <br/> 
        /// The <see cref="Invoice.UniqueInvoiceIdentifier"/> (KSA-1) of the associated Prepayment invoice(s).
        /// </summary>
        public Guid PrepaymentUuid { get; set; }

        /// <summary>
        /// <b>KSA-28</b>, <b>KSA-29</b> 
        /// <br/> 
        /// <see cref="Invoice.InvoiceIssueDateTime"/> (BT-2 and KSA-25) of the associated Prepayment invoice(s).
        /// </summary>
        public DateTimeOffset PrepaymentIssueDateTime { get; set; }

        /// <summary>
        /// <b>BT-129</b>
        /// <br/> 
        /// The quantity of items (goods or services) that is charged in the Invoice line.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// <b>BT-130</b>
        /// <br/> 
        /// The unit of measure that applies to the <see cref="Quantity"/>.
        /// </summary>
        public string? QuantityUnit { get; set; }

        /// <summary>
        /// <b>BT-131</b>
        /// <br/> 
        /// The total amount of the Invoice line, including allowances (discounts). It is the <see cref="ItemNetPrice"/> multiplied with the <see cref="Quantity"/>. <br/>
        /// The amount is "net" without VAT.
        /// </summary>
        public decimal NetAmount { get; set; }

        public LineAllowanceCharge? AllowanceCharge { get; set; }

        /// <summary>
        /// <b>KSA-11</b>
        /// <br/> 
        /// VAT amount as per Article 53.
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// <b>KSA-12</b>
        /// <br/> 
        /// Line amount inclusive VAT. <br/>
        /// Auto-calculated as <see cref="NetAmount"/> + <see cref="VatAmount"/>.
        /// </summary>
        public decimal AmountIncludingVat => NetAmount + VatAmount; // Rule BR-KSA-51

        /// <summary>
        /// <b>KSA-31</b>
        /// <br/> 
        /// The sum total of VAT category taxable amount (BT-116) subject to specific VAT Category code of the associated Prepayment invoice(s).
        /// </summary>
        public decimal PrepaymentVatCategoryTaxableAmount { get; set; }

        /// <summary>
        /// <b>KSA-32</b>
        /// <br/> 
        /// The sum total of VAT category tax amount (BT-117) subject to specific VAT Category code of the associated Prepayment invoice(s). <br/>
        /// Auto-calculated as <see cref="PrepaymentVatCategoryTaxableAmount"/> x <see cref="PrepaymentVatRate"/>.
        /// </summary>
        public decimal PrepaymentVatCategoryTaxAmount => decimal.Round(PrepaymentVatCategoryTaxableAmount * PrepaymentVatRate, 2); // Rule BR-KSA-79

        /// <summary>
        /// <b>BT-153</b>
        /// <br/> 
        /// The description of goods or services as per Article 53 of the VAT Implementing Regulation.
        /// </summary>
        public string? ItemName { get; set; }

        /// <summary>
        /// <b>BT-156</b>
        /// <br/> 
        /// An identifier, assigned by the Buyer, for the item.
        /// </summary>
        public string? ItemBuyerIdentifier { get; set; }

        /// <summary>
        /// <b>BT-155</b>
        /// <br/> 
        /// An identifier, assigned by the Seller, for the item.
        /// </summary>
        public string? ItemSellerIdentifier { get; set; }

        /// <summary>
        /// <b>BT-157</b>
        /// <br/> 
        /// An item identifier based on a registered scheme. <br/>
        /// This should include the product code type and the actual code.This list includes UPC (11 digit, 12 digit, 13 digit EAN), GTIN (14 digit), Customs HS Code and multiple other codes.
        /// </summary>
        public string? ItemStandardIdentifier { get; set; }

        /// <summary>
        /// <b>BT-146</b>
        /// <br/> 
        /// The price of an item, exclusive of VAT, after subtracting item price discount. <br/>
        /// The Item net price has to be equal to <see cref="ItemGrossPrice"/> - <see cref="ItemPriceDiscount"/> when the former is provided (rule BR-KSAEN16931-07)
        /// </summary>
        public decimal ItemNetPrice { get; set; }

        /// <summary>
        /// <b>BT-151</b>
        /// <br/> 
        /// The VAT category code for the invoiced item.
        /// </summary>
        public VatCategory ItemVatCategory { get; set; }

        /// <summary>
        /// <b>BT-152</b>
        /// <br/> 
        /// The VAT rate, represented as percentage that applies to the invoiced item as per Article 53 of the VAT Implementing Regulation.
        /// </summary>
        public decimal ItemVatRate { get; set; }

        /// <summary>
        /// <b>BT-???</b>
        /// <br/> 
        /// This is required if <see cref="ItemVatCategory"/> is non-standard.
        /// </summary>
        public VatExemptionReason ItemVatExemptionReasonCode { get; set; }

        /// <summary>
        /// <b>BT-???</b>
        /// <br/> 
        /// This is required if <see cref="ItemVatCategory"/> is non-standard.
        /// </summary>
        public string? ItemVatExemptionReasonText { get; set; }

        /// <summary>
        /// <b>KSA-33</b>
        /// <br/> 
        /// The VAT category code (BT-118) of the associated Prepayment invoice(s).
        /// </summary>
        public VatCategory PrepaymentVatCategory { get; set; }

        /// <summary>
        /// <b>KSA-34</b>
        /// <br/> 
        /// The VAT rate (BT-119) of the specific <see cref="PrepaymentVatCategory"/> of the associated Prepayment invoice(s).
        /// </summary>
        public decimal PrepaymentVatRate { get; set; }

        /// <summary>
        /// <b>BT-149</b>
        /// <br/> 
        /// The number of item units to which the price applies.
        /// </summary>
        public decimal ItemPriceBaseQuantity { get; set; }

        /// <summary>
        /// <b>BT-150</b>
        /// <br/> 
        /// Unit code of <see cref="ItemPriceBaseQuantity"/>.
        /// </summary>
        public string? ItemPriceBaseQuantityUnit { get; set; }

        /// <summary>
        /// <b>BT-147</b>
        /// <br/> 
        /// The total discount subtracted from the Item gross price to calculate the Item net price. <br/>
        /// Only applies if the discount is provided per unit and if it is not included in the Item gross price. <br/>
        /// This is in the context of the line item price level (not the line item level).
        /// </summary>
        public decimal ItemPriceDiscount { get; set; }

        /// <summary>
        /// <b>BT-148</b>
        /// <br/> 
        /// The unit price, exclusive of VAT, before subtracting <see cref="ItemPriceDiscount"/>.
        /// </summary>
        public decimal ItemGrossPrice { get; set; }
    }
}

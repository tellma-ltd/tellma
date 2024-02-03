namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Subset of the <see href="https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred1001.htm">UN/CEFACT code list 1001</see>
    /// applicable to ZATCA integration.
    /// </summary>
    public enum InvoiceType
    {
        /// <summary>
        /// Document/message for providing credit information to the relevant party.
        /// </summary>
        CreditNote = 381,

        /// <summary>
        /// Document/message for providing debit information to the relevant party.
        /// </summary>
        DebitNote = 383,

        /// <summary>
        /// An invoice for tax purposes.
        /// </summary>
        TaxInvoice = 388,

        /// <summary>
        /// An invoice the invoicee is producing instead of the seller.
        /// </summary>
        PrepaymentInvoice = 386
    }
}

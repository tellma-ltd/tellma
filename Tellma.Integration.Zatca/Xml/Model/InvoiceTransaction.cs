namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Enum representing the various values that can go in <see cref="Invoice.InvoiceTypeTransactions"/> 
    /// attribute, multiple values are supported with the constraints of ZATCA validation rules.
    /// </summary>
    [Flags]
    public enum InvoiceTransaction
    {
        /// <summary>
        /// Standard tax invoice (0100000).
        /// </summary>
        Standard = 1,

        /// <summary>
        /// Simplified tax invoice (0200000).
        /// </summary>
        Simplified = 2,

        /// <summary>
        /// 3rd Party invoice transaction (0010000).
        /// </summary>
        ThirdParty = 4,

        /// <summary>
        /// Nominal invoice transaction (0001000).
        /// </summary>
        Nominal = 8,

        /// <summary>
        /// Exports invoice transaction (0000100).
        /// </summary>
        Exports = 16,

        /// <summary>
        /// Summary invoice transaction (0000010).
        /// </summary>
        Summary = 32,

        /// <summary>
        /// Self-billed invoice transaction (0000001).
        /// </summary>
        SelfBilled = 64
    }
}

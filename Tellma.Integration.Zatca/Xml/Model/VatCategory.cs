namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Permitted VAT categories in ZATCA invoices.
    /// </summary>
    public enum VatCategory
    {
        /// <summary>
        /// (E) Exempt from Tax
        /// </summary>
        ExemptFromTax = 1,

        /// <summary>
        /// (S) Standard rate
        /// </summary>
        StandardRate = 2,

        /// <summary>
        /// (Z) Zero rated goods
        /// </summary>
        ZeroRatedGoods = 3,

        /// <summary>
        /// (O) Services outside scope of tax / Not subject to VAT
        /// </summary>
        NotSubjectToTax = 4
    }
}

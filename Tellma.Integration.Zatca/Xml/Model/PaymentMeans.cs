namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Subset of the <see href="https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred4461.htm">UNTDID 4461 code list</see>
    /// applicable to ZATCA integration.
    /// </summary>
    public enum PaymentMeans
    {
        /// <summary>
        /// Not defined legally enforceable agreement between two or more parties 
        /// (expressing a contractual right or a right to the payment of money).
        /// </summary>
        InstrumentNotDefined = 1,

        /// <summary>
        /// Payment by currency (including bills and coins) in circulation, including checking account deposits.
        /// </summary>
        InCash = 10,

        /// <summary>
        /// Payment by credit movement of funds from one account to another.
        /// </summary>
        CreditTransfer = 30,

        /// <summary>
        /// Payment by an arrangement for settling debts that is operated by the Post Office.
        /// </summary>
        PaymentToBankAccount = 42,

        /// <summary>
        /// Payment by means of a card issued by a bank or other financial institution.
        /// </summary>
        BankCard = 48
    }
}

namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Model for a party taking in the invoice transaction, either the seller or the buyer, as specified in the
    /// <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_EInvoice_Data_Dictionary%20vF.xlsx">E-Invoice Data Dictionary (2023-05-19)</see>.
    /// </summary>
    public class Party
    {
        /// <summary>
        /// Seller ID is one of the list: <br/>
        /// - Commercial registration number <br/>
        /// - MOMRAH license <br/>
        /// - MHRSD license <br/>
        /// - 700 Number <br/>
        /// - MISA license <br/>
        /// - Other Id
        /// <para/>
        /// Buyer ID is one of the list: <br/>
        /// - Tax Identification Number <br/>
        /// - Commercial registration number <br/>
        /// - MOMRAH license <br/>
        /// - MHRSD license <br/>
        /// - 700 Number <br/>
        /// - MISA license <br/>
        /// - National ID <br/>
        /// - GCC ID <br/>
        /// - Iqama Number <br/>
        /// - Passport ID <br/>
        /// - Other Id
        /// <para/>
        /// In case multiple IDs exist then one of the above must be entered following the sequence specified above
        /// </summary>
        public PartyId? Id { get; set; } = new();

        /// <summary>
        /// Party address.
        /// </summary>
        public Address? Address{ get; set; }

        /// <summary>
        /// <b>BT-31</b>, <b>BT-48</b>
        /// <br/> 
        /// VAT identifier - taxpayer entity. Also known as VAT identification number.
        /// </summary>
        public string? VatNumber { get; set; }

        /// <summary>
        /// <b>BT-27</b>, <b>BT-44</b>
        /// <br/> 
        /// Party name.
        /// </summary>
        public string? Name { get; set; }
    }
}

namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Model for a physical address for an invoice party, as specified in the
    /// <see href="https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_EInvoice_Data_Dictionary%20vF.xlsx">E-Invoice Data Dictionary (2023-05-19)</see>.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// <b>BT-35</b>, <b>BT-50</b>
        /// <br/> 
        /// Address line 1 - the main address line in an address.
        /// </summary>
        public string? Street { get; set; }

        /// <summary>
        /// <b>BT-36</b>, <b>BT-51</b>
        /// <br/> 
        /// Address line 2 - an additional address line in an address that can be used to give further details supplementing the main line.
        /// </summary>
        public string? AdditionalStreet { get; set; }

        /// <summary>
        /// <b>KSA-17</b>, <b>KSA-18</b>
        /// <br/> 
        /// Address building number.
        /// </summary>
        public string? BuildingNumber { get; set; }

        /// <summary>
        /// <b>KSA-23</b>, <b>KSA-19</b>
        /// <br/> 
        /// Address additional number
        /// </summary>
        public string? AdditionalNumber { get; set; }

        /// <summary>
        /// <b>BT-37</b>, <b>BT-52</b>
        /// <br/> 
        /// The common name of the city, town or village, where the party's address is located.
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// <b>BT-38</b>, <b>BT-53</b>
        /// <br/> 
        /// Postal code.
        /// </summary>
        public string? PostalCode { get; set; }

        /// <summary>
        /// <b>BT-39</b>, <b>BT-54</b>
        /// <br/> 
        /// Country subdivision.
        /// </summary>
        public string? Province { get; set; }

        /// <summary>
        /// <b>KSA-3</b>, <b>KSA-4</b>
        /// <br/> 
        /// The name of the subdivision of the city, town, or village in which its address is located, such as the name of its district or borough.
        /// </summary>
        public string? District { get; set; }

        /// <summary>
        /// <b>BT-40</b>, <b>BT-55</b>
        /// <br/> 
        /// Country code.
        /// </summary>
        public string? CountryCode { get; set; }
    }
}

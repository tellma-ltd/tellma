namespace Tellma.Integration.Zatca
{
    public class CsrInput
    {
        public CsrInput(
          string commonName,
          string serialNumber,
          string organizationIdentifier,
          string organizationUnitName,
          string organizationName,
          string countryCode,
          string invoiceType,
          string location,
          string industry)
        {
            CommonName = commonName;
            SerialNumber = serialNumber;
            OrganizationIdentifier = organizationIdentifier;
            OrganizationUnitName = organizationUnitName;
            OrganizationName = organizationName;
            CountryCode = countryCode;
            InvoiceType = invoiceType;
            Location = location;
            Industry = industry;
        }

        public string CommonName { get; }

        public string SerialNumber { get; }

        public string OrganizationIdentifier { get; }

        public string OrganizationUnitName { get; }

        public string OrganizationName { get; }

        public string CountryCode { get; }

        public string InvoiceType { get; }

        public string Location { get; }

        public string Industry { get; }
    }
}

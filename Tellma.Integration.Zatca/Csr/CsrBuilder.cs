using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Microsoft;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace Tellma.Integration.Zatca
{
    public partial class CsrBuilder
    {
        private readonly CsrInput _info;

        public CsrBuilder(CsrInput info)
        {
            _info = info;

            string error = ValidateInput();
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException(error);
            }
        }

        public CsrResult GenerateCsr()
        {
            var input = _info;

            AsymmetricCipherKeyPair keyPair = GenerateKeyPair();

            var distinguishedNameKeys = new ArrayList
            {
                X509Name.C,
                X509Name.OU,
                X509Name.O,
                X509Name.CN
            };

            var distinguishedNameValues = new ArrayList
            {
                input.CountryCode,
                input.OrganizationUnitName,
                input.OrganizationName,
                input.CommonName
            };

            var extensionKeys = new ArrayList
            {
                X509Name.Surname,
                X509Name.UID,
                X509Name.T,
                X509Name.L,
                X509Name.BusinessCategory
            };

            var extensionValues = new ArrayList
            {
                input.SerialNumber,
                input.OrganizationIdentifier,
                input.InvoiceType,
                input.Location,
                input.Industry
            };

            X509Name subject = new(distinguishedNameKeys, distinguishedNameValues);
            GeneralNames extValue = new(new GeneralName[1] { new(new X509Name(extensionKeys, extensionValues)) });

            // Add req_extensions
            X509ExtensionsGenerator extGenerator = new();
            extGenerator.AddExtension(MicrosoftObjectIdentifiers.MicrosoftCertTemplateV1, critical: false, new DerOctetString(new DisplayText(2, "ZATCA-Code-Signing")));
            extGenerator.AddExtension(X509Extensions.SubjectAlternativeName, critical: false, extValue);
            X509Extensions req_ext = extGenerator.Generate();
            AttributePkcs req_extensions = new(PkcsObjectIdentifiers.Pkcs9AtExtensionRequest, new DerSet(req_ext));
            Pkcs10CertificationRequest certificationRequest = new(
                signatureAlgorithm: "SHA256withECDSA", 
                subject: subject, 
                publicKey: keyPair.Public, 
                attributes: new DerSet(req_extensions), 
                signingKey: keyPair.Private
            );

            string csrContent = ToPemString(certificationRequest);
            string privateKey = ToPemString(keyPair.Private)
                .Replace("-----BEGIN EC PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("-----END EC PRIVATE KEY-----", "");

            return new(csrContent, privateKey);
        }

        private static AsymmetricCipherKeyPair GenerateKeyPair()
        {
            ECKeyPairGenerator keyPairGenerator = new("ECDSA");
            KeyGenerationParameters parameters = new(new SecureRandom(), 256);
            keyPairGenerator.Init(parameters);
            AsymmetricCipherKeyPair result = keyPairGenerator.GenerateKeyPair();

            return result;
        }

        private static string ToPemString(object obj)
        {
            StringWriter writer = new();
            Org.BouncyCastle.OpenSsl.PemWriter pemWriter = new(writer);
            pemWriter.WriteObject(obj);
            pemWriter.Writer.Flush();

            return writer.ToString();
        }

        private string ValidateInput()
        {
            // Serial Number
            if (string.IsNullOrEmpty(_info.SerialNumber))
                return "Serial number is mandatory field";
            else if (!SerialNumberRegex().Match(_info.SerialNumber).Success)
                return "Invalid serial number, serial number should be in regular expression format (1-...|2-...|3-....)";

            // Org identifier
            if (string.IsNullOrEmpty(_info.OrganizationIdentifier))
                return "Organization identifier is mandatory field";
            else
            {
                if (_info.OrganizationIdentifier.Length != 15)
                    return "Invalid organization identifier, please provide a valid 15 digit of your vat number";
                if (_info.OrganizationIdentifier[..1] != "3")
                    return "Invalid organization identifier, organization identifier should be started with digit 3";
                if (_info.OrganizationIdentifier.Substring(_info.OrganizationIdentifier.Length - 1, 1) != "3")
                    return "Invalid organization identifier, organization identifier should be end with digit 3";
            }

            // Org unit name
            if (string.IsNullOrEmpty(_info.OrganizationUnitName))
                return "Organization unit name is mandatory field";
            else if (_info.OrganizationIdentifier.Substring(10, 1) == "1" && _info.OrganizationUnitName.Length != 10)
                return "Invalid organization unit name, please provide a valid 10 digit of your group tin number";

            // Org name
            if (string.IsNullOrEmpty(_info.OrganizationName))
                return "Organization name is mandatory field";

            // Country code
            if (string.IsNullOrEmpty(_info.CountryCode))
                return "Country code name is mandatory field";
            else if (_info.CountryCode.Length > 3 || _info.CountryCode.Length < 2)
                return "Invalid country code name, please provide a valid country code name";

            // Invoice Type
            if (string.IsNullOrEmpty(_info.InvoiceType))
                return "Invoice type is mandatory field";
            else if (_info.InvoiceType.Length != 4 || !InvoiceTypeRegex().Match(_info.InvoiceType).Success)
                return "Invalid invoice type, please provide a valid invoice type";

            // Location
            if (string.IsNullOrEmpty(_info.Location))
                return "Location is mandatory field";

            // Industry
            if (string.IsNullOrEmpty(_info.Industry))
                return "Industry is mandatory filed";

            return string.Empty;
        }

        [GeneratedRegex("1-(.+)\\|2-(.+)\\|3-(.+)", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex SerialNumberRegex();

        [GeneratedRegex("^[0-1]{4}$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex InvoiceTypeRegex();
    }

    public class CsrResult
    {
        public CsrResult(string csrContent, string privateKey)
        {
            CsrContent = csrContent;
            PrivateKey = privateKey;
        }

        public string CsrContent { get; }
        public string PrivateKey { get; }
    }

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

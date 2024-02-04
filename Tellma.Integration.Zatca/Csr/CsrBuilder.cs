using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Microsoft;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System.Collections;
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
                throw new ZatcaException(error);
            }
        }

        /// <summary>
        /// Generates a Certificate Stamping Request (CSR) based from the given private key.
        /// </summary>
        /// <param name="privateKeyContent">The content of the private key PEM file without the <c>-----BEGIN ...-----</c> header and <c>-----END ...-----</c> footer.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the provided PEM is invalid</exception>
        public CsrResult GenerateCsr(string privateKeyContent)
        {
            string privateKeyPem = @$"-----BEGIN EC PRIVATE KEY-----
{privateKeyContent}
-----END EC PRIVATE KEY-----";

            // Create key pair from PEM content
            using TextReader reader = new StringReader(privateKeyPem);
            PemReader pemReader = new(reader);

            if (pemReader.ReadObject() is ECPrivateKeyParameters privateKey)
            {
                ECDomainParameters domainParameters = privateKey.Parameters;
                ECPublicKeyParameters publicKey = new(privateKey.AlgorithmName, domainParameters.G.Multiply(privateKey.D), domainParameters);
                AsymmetricCipherKeyPair keyPair = new(publicKey, privateKey);

                return GenerateCsrImpl(keyPair);
            }
            else
            {
                // Handle other cases or throw an exception
                throw new InvalidOperationException($"Unsupported PEM format: {privateKeyContent}");
            }
        }

        public CsrResult GenerateCsr()
        {
            // Generate key pair from scratch
            ECKeyPairGenerator keyPairGenerator = new("ECDSA");
            KeyGenerationParameters parameters = new(new SecureRandom(), 256);
            keyPairGenerator.Init(parameters);
            AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateKeyPair();

            return GenerateCsrImpl(keyPair);
        }

        private CsrResult GenerateCsrImpl(AsymmetricCipherKeyPair keyPair)
        {
            var input = _info;

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

        private static string ToPemString(object obj)
        {
            StringWriter writer = new();
            PemWriter pemWriter = new(writer);
            pemWriter.WriteObject(obj);
            pemWriter.Writer.Flush();

            return writer.ToString();
        }

        private string ValidateInput()
        {
            // Serial Number
            if (string.IsNullOrEmpty(_info.SerialNumber))
                return "Serial number is a mandatory field";
            else if (!SerialNumberRegex().Match(_info.SerialNumber).Success)
                return "Invalid serial number, serial number should be in regular expression format (1-...|2-...|3-....)";

            // Org identifier
            if (string.IsNullOrEmpty(_info.OrganizationIdentifier))
                return "Organization identifier is a mandatory field";
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
                return "Organization unit name is a mandatory field";
            else if (_info.OrganizationIdentifier.Substring(10, 1) == "1" && _info.OrganizationUnitName.Length != 10)
                return "Invalid organization unit name, please provide a valid 10 digit of your group tin number";

            // Org name
            if (string.IsNullOrEmpty(_info.OrganizationName))
                return "Organization name is a mandatory field";

            // Country code
            if (string.IsNullOrEmpty(_info.CountryCode))
                return "Country code name is a mandatory field";
            else if (_info.CountryCode.Length > 3 || _info.CountryCode.Length < 2)
                return "Invalid country code name, please provide a valid country code name";

            // Invoice Type
            if (string.IsNullOrEmpty(_info.InvoiceType))
                return "Invoice type is a mandatory field";
            else if (_info.InvoiceType.Length != 4 || !InvoiceTypeRegex().Match(_info.InvoiceType).Success)
                return "Invalid invoice type, please provide a valid invoice type";

            // Location
            if (string.IsNullOrEmpty(_info.Location))
                return "Location is a mandatory field";

            // Industry
            if (string.IsNullOrEmpty(_info.Industry))
                return "Industry is a mandatory filed";

            return string.Empty;
        }

        [GeneratedRegex("1-(.+)\\|2-(.+)\\|3-(.+)", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex SerialNumberRegex();

        [GeneratedRegex("^[0-1]{4}$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex InvoiceTypeRegex();
    }
}

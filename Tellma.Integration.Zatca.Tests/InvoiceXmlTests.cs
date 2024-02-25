using System.Xml;
using System.Xml.Schema;
using Xunit.Abstractions;

namespace Tellma.Integration.Zatca.Tests
{
    public class InvoiceXmlTests(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;

        [Theory(DisplayName = "Generated XML is valid against UBL 2.1 ")]
        [InlineData("FullyPopulated")]
        [InlineData("StandardInvoice")]
        [InlineData("StandardCreditNote")]
        [InlineData("StandardDebitNote")]
        [InlineData("SimplifiedInvoice")]
        [InlineData("SimplifiedCreditNote")]
        [InlineData("SimplifiedDebitNote")]
        public void ValidSchema(string w)
        {
            // Arrange
            var seller = new Party
            {
                Id = new(PartyIdScheme.CommercialRegistration, "454634645645654"),
                Address = new Address
                {
                    Street = "Tahlia St",
                    BuildingNumber = "1820",
                    AdditionalNumber = "123",
                    District = "Riyadh District",
                    City = "Riyadh",
                    PostalCode = "12345",
                    CountryCode = "SA"
                },
                VatNumber = "300951165100003",
                Name = "Spectra International"
            };

            var invoice = w switch
            {
                "FullyPopulated" => TestSamples.FullyPopulatedInvoice(),
                "StandardInvoice" => InvoiceSamples.StandardInvoice(seller),
                "StandardCreditNote" => InvoiceSamples.StandardCreditNote(seller),
                "StandardDebitNote" => InvoiceSamples.StandardDebitNote(seller),
                "SimplifiedInvoice" => InvoiceSamples.SimplifiedInvoice(seller),
                "SimplifiedCreditNote" => InvoiceSamples.SimplifiedCreditNote(seller),
                "SimplifiedDebitNote" => InvoiceSamples.SimplifiedDebitNote(seller),
                _ => throw new Exception(),
            };

            var xml = new InvoiceXml(invoice).Build().GetXml();

            // Act
            var problems = ValidateUblSchema(xml);
            if (problems.Any())
            {
                _output.WriteLine("===== Errors =====");
                foreach (var problem in problems)
                {
                    _output.WriteLine(problem.Message);
                    _output.WriteLine("-----");
                }
            }

            // Print the doc
            _output.WriteLine("===== Document =====");
            _output.WriteLine(xml);

            // Assert
            Assert.Empty(problems);
        }

        [Fact(DisplayName = "Generate XML with a valid model")]
        public void ValidModel()
        {
            var invoice = TestSamples.ValidStandardInvoice();

            var xml = new InvoiceXml(invoice).Build().GetXml();
            _output.WriteLine("===== Document =====");
            _output.WriteLine(xml);
        }

        [Fact(DisplayName = "Generate XML with an empty model")]
        public void EmptyModel()
        {
            var invoice = new Invoice();

            var xml = new InvoiceXml(invoice).Build().GetXml();
            _output.WriteLine("===== Document =====");
            _output.WriteLine(xml);
        }

        [Fact(DisplayName = "Generate XML with a semi-empty model")]
        public void SemiEmptyModel()
        {
            var invoice = new Invoice
            {
                Seller = new(),
                Buyer = new(),
                AllowanceCharges = new(),
                Lines = new() {
                    new InvoiceLine() {
                        AllowanceCharge = new()
                    }
                }
            };

            var xml = new InvoiceXml(invoice).Build().GetXml();
            _output.WriteLine("===== Document =====");
            _output.WriteLine(xml);
        }

        [Fact(DisplayName = "Signing XML is consistent with FATOORA", Skip = "Need to regenerate the expected values")]
        public void SignInvoice()
        {
            // Arrange
            var invoice = TestSamples.ValidStandardInvoice();

            // These were obtained from the FATOORA tool installation.
            const string certificateContent = "MIID6jCCA5CgAwIBAgITbwAAgbuRbo5tpQ+QjgABAACBuzAKBggqhkjOPQQDAjBjMRUwEwYKCZImiZPyLGQBGRYFbG9jYWwxEzARBgoJkiaJk/IsZAEZFgNnb3YxFzAVBgoJkiaJk/IsZAEZFgdleHRnYXp0MRwwGgYDVQQDExNUU1pFSU5WT0lDRS1TdWJDQS0xMB4XDTIyMTEwOTA4MDcyMloXDTI0MTEwODA4MDcyMlowTjELMAkGA1UEBhMCU0ExEzARBgNVBAoTCjM5OTk5OTk5OTkxDDAKBgNVBAsTA1RTVDEcMBoGA1UEAxMTVFNULTM5OTk5OTk5OTkwMDAwMzBWMBAGByqGSM49AgEGBSuBBAAKA0IABGGDDKDmhWAITDv7LXqLX2cmr6+qddUkpcLCvWs5rC2O29W/hS4ajAK4Qdnahym6MaijX75Cg3j4aao7ouYXJ9GjggI5MIICNTCBmgYDVR0RBIGSMIGPpIGMMIGJMTswOQYDVQQEDDIxLVRTVHwyLVRTVHwzLTlmMDkyMjM4LTFkOTctNDcxOC1iNDQxLWNiYzMwMTMyMWIwYTEfMB0GCgmSJomT8ixkAQEMDzM5OTk5OTk5OTkwMDAwMzENMAsGA1UEDAwEMTEwMDEMMAoGA1UEGgwDVFNUMQwwCgYDVQQPDANUU1QwHQYDVR0OBBYEFDuWYlOzWpFN3no1WtyNktQdrA8JMB8GA1UdIwQYMBaAFHZgjPsGoKxnVzWdz5qspyuZNbUvME4GA1UdHwRHMEUwQ6BBoD+GPWh0dHA6Ly90c3RjcmwuemF0Y2EuZ292LnNhL0NlcnRFbnJvbGwvVFNaRUlOVk9JQ0UtU3ViQ0EtMS5jcmwwga0GCCsGAQUFBwEBBIGgMIGdMG4GCCsGAQUFBzABhmJodHRwOi8vdHN0Y3JsLnphdGNhLmdvdi5zYS9DZXJ0RW5yb2xsL1RTWkVpbnZvaWNlU0NBMS5leHRnYXp0Lmdvdi5sb2NhbF9UU1pFSU5WT0lDRS1TdWJDQS0xKDEpLmNydDArBggrBgEFBQcwAYYfaHR0cDovL3RzdGNybC56YXRjYS5nb3Yuc2Evb2NzcDAOBgNVHQ8BAf8EBAMCB4AwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMDMCcGCSsGAQQBgjcVCgQaMBgwCgYIKwYBBQUHAwIwCgYIKwYBBQUHAwMwCgYIKoZIzj0EAwIDSAAwRQIgeWUEjxXaW4s8XilH/abzbDJhHHjO3uLaD87YqioA89YCIQDNltfAU98b8FnTD7M8NYIk8cqi7OnPu7h85v5V1Bt3Hg==";
            const string privateKeyContent = "MHQCAQEEIDyLDaWIn/1/g3PGLrwupV4nTiiLKM59UEqUch1vDfhpoAcGBSuBBAAKoUQDQgAEYYMMoOaFYAhMO/steotfZyavr6p11SSlwsK9azmsLY7b1b+FLhqMArhB2dqHKboxqKNfvkKDePhpqjui5hcn0Q==";
            const string signingTime = "2023-12-31T18:28:37";

            // Act
            var sigInfo = new TestingInvoiceXmlBuilder(invoice, signingTime)
                .Build()
                .Sign(certificateContent, privateKeyContent);

            // Assert
            Assert.Equal(signingTime, sigInfo.SigningTime);

            // These were obtained by running the FATOORA tool
            const string expectedInvoiceHash = "TqSGxNUkKSPJl85DQ8vPsGhL0rhB0Oj19dI8QgZgMYo=";
            const string expectedSignedPropertiesHash = "YWM3ZDAxODk0YmVkOWQ4OWRmMGM0YTIyNTYzNzhjNTU2NDQ5ZGU5ZmQ5NjI0ZTVmNzRlYjI5MTAwYzZhODg0NA==";
            const string expectedIssuerName = "CN=TSZEINVOICE-SubCA-1, DC=extgazt, DC=gov, DC=local";
            const string expectedCertHash = "Y2U5MzY5MTFiOTA4NTc0YmI2NjExNDFlMzBkNmM2YTljZWMxYjRlZDFmYWE3NjE1NjVlNDQzNjA3ODdkYzZjZQ==";
            const string expectedSerialNumber = "2475382889481219846080454947234981286678397371";

            Assert.Equal(expectedInvoiceHash, sigInfo.InvoiceHash);
            Assert.Equal(expectedSignedPropertiesHash, sigInfo.SignedPropertiesHash);
            Assert.Equal(expectedIssuerName, sigInfo.CertificateIssuerName);
            Assert.Equal(expectedSerialNumber, sigInfo.CertificateSerialNumber);
            Assert.Equal(expectedCertHash, sigInfo.CertificateHash);
        }

        #region Helpers

        internal class TestingInvoiceXmlBuilder : InvoiceXml
        {
            private string _signingTime = "2023-12-31T18:28:37";

            public TestingInvoiceXmlBuilder(Invoice inv, string signingTime) : base(inv)
            {
                _signingTime = signingTime;
            }
            protected override string GetCurrentTime() => _signingTime;
            public override string GetXml() => File.ReadAllText("Resources/StandardInvoice_FatooraSample_Unsigned.xml");
        }

        private static IEnumerable<ValidationResult> ValidateUblSchema(string xml)
        {
            List<ValidationResult> result = new();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            // Add all the XSDs
            xmlDoc.Schemas.Add(null, "https://docs.oasis-open.org/ubl/os-UBL-2.1/xsd/maindoc/UBL-Invoice-2.1.xsd");
            xmlDoc.Schemas.Add(null, "https://docs.oasis-open.org/ubl/os-UBL-2.1/xsd/common/UBL-CommonExtensionComponents-2.1.xsd");
            xmlDoc.Schemas.Add(null, "https://docs.oasis-open.org/ubl/os-UBL-2.1/xsd/common/UBL-UnqualifiedDataTypes-2.1.xsd");
            xmlDoc.Schemas.Add(null, "https://docs.oasis-open.org/ubl/os-UBL-2.1/xsd/common/UBL-CommonBasicComponents-2.1.xsd");
            xmlDoc.Schemas.Add(null, "https://docs.oasis-open.org/ubl/os-UBL-2.1/xsd/common/UBL-CommonAggregateComponents-2.1.xsd");
            xmlDoc.Schemas.Add(null, "https://docs.oasis-open.org/ubl/os-UBL-2.1/xsd/common/UBL-QualifiedDataTypes-2.1.xsd");
            xmlDoc.Schemas.Add(null, "https://docs.oasis-open.org/ubl/os-UBL-2.1/xsd/common/UBL-ExtensionContentDataType-2.1.xsd");
            xmlDoc.Schemas.Add(null, "https://docs.oasis-open.org/ubl/os-UBL-2.1/xsd/common/CCTS_CCT_SchemaModule-2.1.xsd");

            xmlDoc.Validate((s, e) => result.Add(new(e.Severity, e.Message)));

            return result;
        }

        /// <summary>
        /// Helper class for <see cref="ValidateUblSchema(XmlDocument)"/>.
        /// </summary>
        public class ValidationResult
        {
            public ValidationResult(XmlSeverityType sev, string msg)
            {
                Message = msg;
                Severity = sev;
            }

            public string Message { get; }
            public XmlSeverityType Severity { get; }
        }

        #endregion
    }
}

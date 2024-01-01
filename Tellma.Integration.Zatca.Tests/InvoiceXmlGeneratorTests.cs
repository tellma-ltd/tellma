using System.Xml;
using System.Xml.Schema;
using Xunit.Abstractions;

namespace Tellma.Integration.Zatca.Tests
{
    public class InvoiceXmlGeneratorTests
    {
        private readonly ITestOutputHelper _output;

        public InvoiceXmlGeneratorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(DisplayName = "Generated XML is valid against UBL 2.1")]
        public void ValidSchema()
        {
            var invoice = InvoicesSamples.FullyPopulatedInvoice();
            var xml = new InvoiceXmlBuilder(invoice).Build().GetXml();

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

        [Fact(DisplayName = "Generating XML with a valid model")]
        public void ValidModel()
        {
            var invoice = InvoicesSamples.ValidStandardInvoice();

            var xml = new InvoiceXmlBuilder(invoice).Build().GetXml();
            _output.WriteLine(xml);
        }

        [Fact(DisplayName = "Generating XML with an empty model")]
        public void EmptyModel()
        {
            var invoice = new Invoice();

            var xml = new InvoiceXmlBuilder(invoice).Build().GetXml();
            _output.WriteLine(xml);
        }

        [Fact(DisplayName = "Generating XML with a semi-empty model")]
        public void SemiEmptyModel()
        {
            var invoice = new Invoice
            {
                Seller = new(),
                Buyer = new(),
                AllowanceCharge = new(),
                Lines = new() {
                    new InvoiceLine() {
                        AllowanceCharge = new()
                    }
                }
            };

            var xml = new InvoiceXmlBuilder(invoice).Build().GetXml();
            _output.WriteLine(xml);
        }

        #region Helpers

        /// <summary>
        /// Validates the given XML against the UBL 2.1 schema, and returns any validation errors.
        /// </summary>
        public static IEnumerable<ValidationResult> ValidateUblSchema(string xml)
        {
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

            List<ValidationResult> result = new();
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

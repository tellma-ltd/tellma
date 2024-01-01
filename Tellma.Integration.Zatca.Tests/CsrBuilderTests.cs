using Xunit.Abstractions;

namespace Tellma.Integration.Zatca.Tests
{
    public class CsrBuilderTests
    {
        private readonly ITestOutputHelper _output;

        public CsrBuilderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(DisplayName = "Generate CSR with valid input")]
        public void ValidInvoice()
        {
            // Arrange
            var input = new CsrInput(
                commonName: "127.0.0.1",
                serialNumber: "1-Tellma|2-web.tellma.com|3-101",
                organizationIdentifier: "310175397500003",
                organizationUnitName: "Riyad Branch",
                organizationName: "Tellma",
                countryCode: "SA",
                invoiceType: "1100",
                location: "Tahlia St",
                industry: "Software"
            );

            // Act
            var result = new CsrBuilder(input).GenerateCsr();

            // Assert
            // ...

            _output.WriteLine("===== CSR Content =====");
            _output.WriteLine(result.CsrContent);

            _output.WriteLine("===== Private Key =====");
            _output.WriteLine(result.PrivateKey);
        }

        [Fact(DisplayName = "Generate CSR with invalid input")]
        public void InvalidInvoice()
        {
            // Arrange
            var input = new CsrInput(
                commonName: "127.0.0.1",
                serialNumber: "Foobar",
                organizationIdentifier: "310175397500003",
                organizationUnitName: "Riyad Branch",
                organizationName: "Tellma",
                countryCode: "SA",
                invoiceType: "1100",
                location: "Tahlia St",
                industry: "Software"
            );

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                var result = new CsrBuilder(input).GenerateCsr();
            });
        }
    }
}

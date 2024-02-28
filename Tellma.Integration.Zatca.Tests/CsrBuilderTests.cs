using Xunit.Abstractions;

namespace Tellma.Integration.Zatca.Tests
{
    public class CsrBuilderTests(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;
        private const string privateKey = "MHQCAQEEIA4n00MZGH7W9id05A2qcJvG31tR7wmqeHnrPwv49t8coAcGBSuBBAAKoUQDQgAE4+IY3+PItL/4WVzAZxpBLxcx0TaOWuVmjXFeXd7wsC9VteKsgAmWPb25KXqVG6f+wJdZHRkTTCR0GC4i1PhcKA==";

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
            var result = new CsrBuilder(input).GenerateCsr(privateKey);

            // Assert
            // ...

            _output.WriteLine("===== CSR Content =====");
            _output.WriteLine(result);
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
            Assert.Throws<ZatcaException>(() =>
            {
                var result = new CsrBuilder(input).GenerateCsr(privateKey);
            });
        }
    }
}

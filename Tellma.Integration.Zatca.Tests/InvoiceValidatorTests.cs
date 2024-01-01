using Xunit.Abstractions;

namespace Tellma.Integration.Zatca.Tests
{
    public class InvoiceValidatorTests
    {
        private readonly ITestOutputHelper _output;

        public InvoiceValidatorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(DisplayName = "Valid invoice passes validation")]
        public void ValidInvoice()
        {
            // Arrange
            var invoice = InvoicesSamples.ValidStandardInvoice();
            var validator = new InvoiceValidator(invoice);

            // Act
            var results = validator.Validate();

            foreach (var e in results)
                _output.WriteLine(e.Rule + ": " + e.Message);

            // Assert
            Assert.Empty(results);
        }

        [Fact(DisplayName = "Invalid invoice fails validation")]
        public void InvalidInvoice()
        {
            // Arrange
            var invoice = InvoicesSamples.ValidStandardInvoice();

            invoice.Seller = null;
            var validator = new InvoiceValidator(invoice);

            // Act
            var results = validator.Validate();

            foreach (var e in results)
                _output.WriteLine(e.Rule + ": " + e.Message);

            // Assert
            Assert.NotEmpty(results);
        }
    }
}

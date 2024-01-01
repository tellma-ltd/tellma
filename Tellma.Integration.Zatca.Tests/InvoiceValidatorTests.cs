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
    }
}

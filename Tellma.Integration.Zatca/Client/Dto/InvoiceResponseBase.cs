using System.Text.Json.Serialization;

namespace Tellma.Integration.Zatca
{
    public class InvoiceResponseBase
    {
        [JsonPropertyName("validationResults")]
        public ResponseValidationResults ValidationResults { get; set; } = new();
    }
}
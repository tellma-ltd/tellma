using System.Text.Json.Serialization;

namespace Tellma.Integration.Zatca
{
    public class InvoiceRequestBase
    {
        [JsonPropertyName("invoice")]
        public string? Invoice { get; set; }

        [JsonPropertyName("invoiceHash")]
        public string? InvoiceHash { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("uuid")]
        public Guid Uuid { get; set; }
    }
}
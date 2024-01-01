using System.Text.Json.Serialization;

namespace Tellma.Integration.Zatca
{
    public class CsrRequest
    {
        [JsonPropertyName("csr")]
        public string? Csr { get; set; }
    }
}
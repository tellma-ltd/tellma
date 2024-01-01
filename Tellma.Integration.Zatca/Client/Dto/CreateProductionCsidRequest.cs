using System.Text.Json.Serialization;

namespace Tellma.Integration.Zatca
{
    public class CreateProductionCsidRequest
    {
        [JsonPropertyName("compliance_request_id")]
        public string? ComplianceRequestId { get; set; }
    }
}
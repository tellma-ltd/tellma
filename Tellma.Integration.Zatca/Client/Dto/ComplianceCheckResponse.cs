using System.Text.Json.Serialization;

namespace Tellma.Integration.Zatca
{
    public class ComplianceCheckResponse : InvoiceResponseBase
    {
        /// <summary>
        /// <see cref="Constants.ReportingStatus"/>
        /// </summary>
        [JsonPropertyName("reportingStatus")]
        public string? ReportingStatus { get; set; }

        /// <summary>
        /// <see cref="Constants.ClearanceStatus"/>
        /// </summary>
        [JsonPropertyName("clearanceStatus")]
        public string? ClearanceStatus { get; set; }

        [JsonPropertyName("qrSellerStatus")]
        public string? QrSellerStatus { get; set; }

        [JsonPropertyName("qrBuyerStatus")]
        public string? QrBuyerStatus { get; set; }
    }
}
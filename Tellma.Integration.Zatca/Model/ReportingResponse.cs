using System.Text.Json.Serialization;

namespace Tellma.Integration.Zatca
{
    public class ReportingResponse : InvoiceResponseBase
    {
        /// <summary>
        /// <see cref="Constants.ReportingStatus"/>
        /// </summary>
        [JsonPropertyName("reportingStatus")]
        public string? ReportingStatus { get; set; }

        public bool IsReported => ReportingStatus == Constants.ReportingStatus.REPORTED;
    }
}
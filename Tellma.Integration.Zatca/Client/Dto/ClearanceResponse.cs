using System.Text.Json.Serialization;

namespace Tellma.Integration.Zatca
{
    public class ClearanceResponse : InvoiceResponseBase
    {
        /// <summary>
        /// <see cref="Constants.ClearanceStatus"/>
        /// </summary>
        [JsonPropertyName("clearanceStatus")]
        public string? ClearanceStatus { get; set; }

        [JsonPropertyName("clearedInvoice")]
        public string? ClearedInvoice { get; set; }

        public bool IsCleared => ClearanceStatus == Constants.ClearanceStatus.CLEARED;
    }
}
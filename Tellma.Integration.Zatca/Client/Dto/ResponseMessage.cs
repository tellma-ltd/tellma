using System.Text.Json.Serialization;

namespace Tellma.Integration.Zatca
{
    public class ResponseMessage
    {
        /// <summary>
        /// <see cref="Constants.ValidationType"/>
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// e.g. XSD_ZATCA_VALID, invoiceTimeStamp_QRCODE_INVALID, invalid-signing-certificate, XSD_ZATCA_INVALID
        /// </summary>
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        /// <summary>
        /// e.g. XSD validation, QRCODE_VALIDATION
        /// </summary>
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// <see cref="Constants.ValidationStatus"/>
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
using System.Text.Json.Serialization;

namespace Tellma.Integration.Zatca
{
    public class ResponseValidationResults
    {
        [JsonPropertyName("infoMessages")]
        public List<ResponseMessage> InfoMessages { get; set; } = new();

        [JsonPropertyName("warningMessages")]
        public List<ResponseMessage> WarningMessages { get; set; } = new();

        [JsonPropertyName("errorMessages")]
        public List<ResponseMessage> ErrorMessages { get; set; } = new();

        /// <summary>
        /// <see cref="Constants.ValidationStatus"/>
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
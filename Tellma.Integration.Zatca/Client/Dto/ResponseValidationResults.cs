using System.Text.Json.Serialization;

namespace Tellma.Integration.Zatca
{
    public class ResponseValidationResults
    {
        [JsonPropertyName("infoMessages")]
        public List<ResponseMessage> InfoMessages { get; set; } = [];

        [JsonPropertyName("warningMessages")]
        public List<ResponseMessage> WarningMessages { get; set; } = [];

        [JsonPropertyName("errorMessages")]
        public List<ResponseMessage> ErrorMessages { get; set; } = [];

        /// <summary>
        /// <see cref="Constants.ValidationStatus"/>
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
using System.Text.Json.Serialization;

namespace Tellma.Integration.Zatca
{
    public class CsidResponse
    {
        [JsonPropertyName("requestID")]
        public long RequestId { get; set; }

        /// <summary>
        /// <see cref="Constants.Disposition"/>
        /// </summary>
        [JsonPropertyName("dispositionMessage")]
        public string? DispositionMessage { get; set; }

        [JsonPropertyName("binarySecurityToken")]
        public string? BinarySecurityToken { get; set; }

        [JsonPropertyName("secret")]
        public string? Secret { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }
    }
}
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Tellma.Utilities.SendGrid
{
    /// <summary>
    /// This class models the data that SendGrid posts from their event webhook stream.<br/>
    /// Docs: https://sendgrid.com/docs/for-developers/tracking-events/event/.
    /// </summary>
    public class SendGridEventNotification
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("uid")]
        public int Uid { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("sendgrid_event_id")]
        public string SendGridEventId { get; set; }

        [JsonPropertyName("smtp-id")]
        public string SmtpId { get; set; }

        [JsonPropertyName("sg_message_id")]
        public string SgMessageId { get; set; }

        [JsonPropertyName("event")] // event is a protected keyword
        public string Event { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("category")]
        public IList<string> Category { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("useragent")]
        public string UserAgent { get; set; }

        [JsonPropertyName("ip")]
        public string Ip { get; set; }

        // Custom Fields

        [JsonPropertyName(SendGridEmailSender.EmailIdKey)]
        public int EmailId { get; set; }

        [JsonPropertyName(SendGridEmailSender.TenantIdKey)]
        public int? TenantId { get; set; }
    }
}

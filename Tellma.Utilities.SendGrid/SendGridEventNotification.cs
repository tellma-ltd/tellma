using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tellma.Utilities.SendGrid
{
    /// <summary>
    /// This class models the data that SendGrid posts from their event webhook stream.<br/>
    /// Docs: https://sendgrid.com/docs/for-developers/tracking-events/event/.
    /// </summary>
    public class SendGridEventNotification
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("uid")]
        public int Uid { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("sendgrid_event_id")]
        public string SendGridEventId { get; set; }

        [JsonProperty("smtp-id")] // switched to underscore for consistancy
        public string SmtpId { get; set; }

        [JsonProperty("sg_message_id")]
        public string SgMessageId { get; set; }

        [JsonProperty("event")] // event is a protected keyword
        public string Event { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("category")]
        public IList<string> Category { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("useragent")]
        public string UserAgent { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        // Custom Fields

        [JsonProperty(SendGridEmailSender.EmailIdKey)]
        public int EmailId { get; set; }

        [JsonProperty(SendGridEmailSender.TenantIdKey)]
        public int? TenantId { get; set; }
    }
}

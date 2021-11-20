using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tellma.Api.Notifications
{
    /// <summary>
    /// DTO that matches the structure of the standard web push notification as explained here https://mzl.la/3kCDpaD. <br/>
    /// The DTO is designed to be convertable to JSON using the JSON.NET library.
    /// </summary>
    public class PushContent
    {
        [JsonPropertyName("title")]
        public string Title { get; } // Required

        [JsonPropertyName("badge")]
        public string Badge { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("data")]
        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        [JsonPropertyName("dir")]
        public string Dir { get; set; } // "auto"|"ltr"|"rtl"

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("lang")]
        public string Lang { get; set; }

        [JsonPropertyName("renotify")]
        public bool? Renotify { get; set; }

        [JsonPropertyName("requireInteraction")]
        public bool? RequireInteraction { get; set; }

        [JsonPropertyName("silent")]
        public bool? Silent { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; }

        [JsonPropertyName("vibrate")]
        public List<int> Vibrate { get; set; }

        [JsonPropertyName("actions")]
        public List<NotificationAction> Actions { get; set; } = new List<NotificationAction>();

        public class NotificationAction
        {
            [JsonPropertyName("action")]
            public string Action { get; } // Required

            [JsonPropertyName("title")]
            public string Title { get; } // Required
        }
    }
}

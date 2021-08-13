using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tellma.Api.Notifications
{
    /// <summary>
    /// DTO that matches the structure of the standard web push notification as explained here https://mzl.la/3kCDpaD. <br/>
    /// The DTO is designed to be convertable to JSON using the JSON.NET library.
    /// </summary>
    public class PushContent
    {
        [JsonProperty("title")]
        public string Title { get; } // Required

        [JsonProperty("badge")]
        public string Badge { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("data")]
        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        [JsonProperty("dir")]
        public string Dir { get; set; } // "auto"|"ltr"|"rtl"

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("lang")]
        public string Lang { get; set; }

        [JsonProperty("renotify")]
        public bool? Renotify { get; set; }

        [JsonProperty("requireInteraction")]
        public bool? RequireInteraction { get; set; }

        [JsonProperty("silent")]
        public bool? Silent { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty("vibrate")]
        public IList<int> Vibrate { get; set; }

        [JsonProperty("actions")]
        public IList<NotificationAction> Actions { get; set; } = new List<NotificationAction>();

        public class NotificationAction
        {
            [JsonProperty("action")]
            public string Action { get; } // Required

            [JsonProperty("title")]
            public string Title { get; } // Required
        }
    }
}

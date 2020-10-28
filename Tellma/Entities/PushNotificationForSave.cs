using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "PushNotification", Plural = "PushNotifications")]
    public class PushNotificationForSave : EntityWithKey<int>
    {
        public string Endpoint { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }

        [Display(Name = "PushNotification_Title")]
        public string Title { get; set; } // Also contained in Content JSON

        [Display(Name = "PushNotification_Body")]
        public string Body { get; set; } // Also contained in Content JSON

        public string Content { get; set; } // JSON
    }

    public class PushNotification : PushNotificationForSave
    {
        // TODO: State and timestamps 
    }
}

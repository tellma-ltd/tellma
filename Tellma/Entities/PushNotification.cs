using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "PushNotification", Plural = "PushNotifications")]
    public class PushNotification : EntityWithKey<int>
    {
        public string Endpoint { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }

        [Display(Name = "PushNotification_Content")]
        public string Content { get; set; } // JSON

        // TODO: State and timestamps
    }
}

namespace Tellma.Controllers.Jobs
{
    public class PushNotificationQueueItem
    {
        public string Endpoint { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }
        public PushNotificationInfo Content { get; set; }
        public int PushId { get; set; }
        public int TenantId { get; set; }
    }
}

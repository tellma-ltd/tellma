namespace Tellma.Api.Notifications
{
    /// <summary>
    /// Represent a single web push notification to an end user device
    /// </summary>
    public class PushToSend // TODO Move to push notifications service
    {
        public string Endpoint { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }
        public PushContent Content { get; set; }
        public int PushId { get; set; }
        public int TenantId { get; set; }
    }
}

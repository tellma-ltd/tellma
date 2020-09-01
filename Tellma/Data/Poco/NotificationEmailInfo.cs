namespace Tellma.Data
{
    public class NotificationEmailInfo
    {
        public string ToEmail { get; set; }
        public string FromEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int EmailId { get; set; }
        public int TenantId { get; set; }
    }
}

namespace Tellma.Api.Notifications
{
    public class NotificationCommandToSend
    {
        public int TemplateId { get; set; }
        public int? EntityId { get; set; }
        public string Caption { get; set; }
        public string Arguments { get; set; }
        public int? CreatedById { get; set; }
    }
}

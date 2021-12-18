namespace Tellma.Api.Notifications
{
    public class NotificationCommandToSend
    {
        public NotificationCommandToSend(int templateId)
        {
            TemplateId = templateId; // the only required property
        }

        public int TemplateId { get; }
        public int? EntityId { get; set; }
        public string Caption { get; set; }
        public int? CreatedById { get; set; }
    }
}

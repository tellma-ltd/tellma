using System;

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
        public DateTimeOffset? ScheduledTime { get; set; }
        public string Arguments { get; set; }
        public int? CreatedById { get; set; }

        /// <summary>
        /// Output parameter
        /// </summary>
        public int EmailCommandId { get; set; }

        /// <summary>
        /// Output parameter
        /// </summary>
        public int MessageCommandId { get; set; }
    }
}

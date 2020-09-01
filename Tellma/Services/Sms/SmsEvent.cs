using System;

namespace Tellma.Services.Sms
{
    public class SmsEvent
    {
        public int MessageId { get; set; }
        public int? TenantId { get; set; }
        public SmsEventType Type { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}

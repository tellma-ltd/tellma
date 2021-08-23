using System;

namespace Tellma.Utilities.Sms
{
    /// <summary>
    /// A DTO class that represent a notification from the SMS service (e.g. Twilio) on an event that occurred during SMS processing.
    /// For example delivery to the carrier network or delivery to the end user device.
    /// </summary>
    public class SmsEventNotification
    {
        /// <summary>
        /// The Id of the <see cref="SmsToSend"/> that was sent (if any)
        /// </summary>
        public int MessageId { get; set; }

        /// <summary>
        /// The Tenant Id of the <see cref="SmsToSend"/> that was sent (if any)
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// The <see cref="SmsEvent"/> that occurred
        /// </summary>
        public SmsEvent Event { get; set; }

        /// <summary>
        /// For negative <see cref="SmsEvent"/>s, this would be the accompanying error
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// When was the event notification received
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }
}

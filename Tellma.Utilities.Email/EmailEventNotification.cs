using System;

namespace Tellma.Utilities.Email
{
    /// <summary>
    /// A DTO class that represent a notification from the email service (e.g. SendGrid) on an event that occurred during Email processing.
    /// For example delivery to the recipient server or an email bouncing, etc...
    /// </summary>
    public class EmailEventNotification
    {
        /// <summary>
        /// The Id of the <see cref="EmailToSend"/> that was sent (if any)
        /// </summary>
        public int EmailId { get; set; }

        /// <summary>
        /// The Tenant Id of the <see cref="EmailToSend"/> that was sent (if any)
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// The <see cref="EmailEvent"/> that occurred
        /// </summary>
        public EmailEvent Event { get; set; }

        /// <summary>
        /// For negative <see cref="EmailEvent"/>s, this would be the accompanying error
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// When was the event notification received
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }
}

using System;

namespace Tellma.Utilities.Sms
{
    public class SmsToSend
    {
        public SmsToSend(string phoneNumber, string content)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException($"'{nameof(phoneNumber)}' cannot be null or whitespace", nameof(phoneNumber));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException($"'{nameof(content)}' cannot be null or whitespace", nameof(content));
            }

            PhoneNumber = phoneNumber;
            Content = content;
        }

        /// <summary>
        /// The phone number to send the SMS to
        /// </summary>
        public string PhoneNumber { get; }

        /// <summary>
        /// The content of the SMS
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// The Id of the tenant where the message is stored
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// The message id in the tenant database
        /// </summary>
        public int MessageId { get; set; }
    }
}

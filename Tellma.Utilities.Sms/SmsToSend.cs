using System;

namespace Tellma.Utilities.Sms
{
    public class SmsToSend
    {
        public SmsToSend(string toPhoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
            {
                throw new ArgumentException($"'{nameof(toPhoneNumber)}' cannot be null or whitespace", nameof(toPhoneNumber));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException($"'{nameof(message)}' cannot be null or whitespace", nameof(message));
            }

            ToPhoneNumber = toPhoneNumber;
            Message = message;
        }

        /// <summary>
        /// The phone number to send the SMS to
        /// </summary>
        public string ToPhoneNumber { get; }

        /// <summary>
        /// The content of the SMS
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The message id in the tenant database
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// The Id of the tenant where the message is stored
        /// </summary>
        public int MessageId { get; set; }
    }
}

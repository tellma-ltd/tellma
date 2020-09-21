using System;

namespace Tellma.Services.Sms
{
    public class SmsForSender
    {
        public SmsForSender(string toPhoneNumber, string message)
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

        public string ToPhoneNumber { get; }
        public string Message { get; }
        public int? TenantId { get; set; }
        public int? MessageId { get; set; }
    }
}

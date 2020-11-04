using System.ComponentModel.DataAnnotations;

namespace Tellma.Services.Sms
{
    public static class SmsValidation
    {
        private static readonly PhoneAttribute Phone = new PhoneAttribute();

        public const int MaximumPhoneNumberLength = 15;
        public const int MaximumSmsLength = 500;

        /// <summary>
        /// If the <see cref="SmsMessage"/> is valid, returns null, otherwise returns the validation error message.
        /// </summary>
        public static string Validate(SmsMessage sms)
        {
            if (string.IsNullOrWhiteSpace(sms.ToPhoneNumber))
            {
                return $"Missing phone number.";
            }
            
            if (!Phone.IsValid(sms.ToPhoneNumber))
            {
                return $"Invalid phone number '{sms.ToPhoneNumber}'.";
            }          
            
            if (sms.ToPhoneNumber.Length > MaximumPhoneNumberLength)
            {
                return $"Phone number exceeds the maximum possible length of {MaximumPhoneNumberLength}.";
            }
            
            if (string.IsNullOrWhiteSpace(sms.Message))
            {
                return $"Empty message.";
            }
            
            if (sms.Message.Length > MaximumSmsLength)
            {
                return $"Message length exceeds the maximum possible length of {MaximumSmsLength}.";
            }

            return null;
        }
    }
}

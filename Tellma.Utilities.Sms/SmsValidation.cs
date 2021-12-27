using System.ComponentModel.DataAnnotations;

namespace Tellma.Utilities.Sms
{
    public static class SmsValidation
    {
        private static readonly PhoneAttribute Phone = new PhoneAttribute();

        public const int MaximumPhoneNumberLength = 15;
        public const int MaximumSmsLength = 500;

        /// <summary>
        /// If the <see cref="SmsToSend"/> is valid, returns null, otherwise returns the validation error message.
        /// </summary>
        public static string Validate(SmsToSend sms)
        {
            if (string.IsNullOrWhiteSpace(sms.PhoneNumber))
            {
                return $"Missing phone number.";
            }
            
            if (!Phone.IsValid(sms.PhoneNumber))
            {
                return $"Invalid phone number '{sms.PhoneNumber}'.";
            }          
            
            if (sms.PhoneNumber.Length > MaximumPhoneNumberLength)
            {
                return $"Phone number exceeds the maximum possible length of {MaximumPhoneNumberLength}.";
            }
            
            if (string.IsNullOrWhiteSpace(sms.Content))
            {
                return $"Empty message.";
            }
            
            if (sms.Content.Length > MaximumSmsLength)
            {
                return $"Message length exceeds the maximum possible length of {MaximumSmsLength}.";
            }

            return null;
        }
    }
}

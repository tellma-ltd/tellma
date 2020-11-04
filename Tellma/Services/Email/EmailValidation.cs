using System.ComponentModel.DataAnnotations;

namespace Tellma.Services.Email
{
    public static class EmailValidation
    {
        private static readonly EmailAddressAttribute EmailAtt = new EmailAddressAttribute();

        public const int MaximumEmailAddressLength = 256;
        public const int MaximumSubjectLength = 1024;
        public const int MaximumBodyLength = 50 * 1024; // 50 KB

        /// <summary>
        /// If the <see cref="SmsMessage"/> is valid, returns null, otherwise returns the validation error message.
        /// </summary>
        public static string Validate(Email email)
        {
            if (string.IsNullOrWhiteSpace(email.ToEmail))
            {
                return $"Missing to email address.";
            }

            if (email.ToEmail.Length > MaximumEmailAddressLength)
            {
                return $"Email address exceeds maximum size of {FileSizeDisplay(MaximumEmailAddressLength)}";
            }

            if (email.Subject != null && email.Subject.Length > MaximumSubjectLength)
            {
                return $"Email subject exceeds maximum size of {FileSizeDisplay(MaximumSubjectLength)}";
            }

            if (!EmailAtt.IsValid(email.ToEmail))
            {
                return $"Invalid email address '{email.ToEmail}'.";
            }

            if (string.IsNullOrWhiteSpace(email.Subject) && string.IsNullOrWhiteSpace(email.Body))
            {
                return $"Both subject and body are empty.";
            }

            if (email.Body != null && email.Body.Length > MaximumBodyLength)
            {
                return $"Email body exceeds the maximum size of {FileSizeDisplay(MaximumBodyLength)}.";
            }

            return null;
        }

        private static readonly string [] Sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        
        private  static string FileSizeDisplay(long fileSize)
        {
            double len = fileSize;
            int order = 0;
            while (len >= 1024 && order < Sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.#} {Sizes[order]}";
        }
    }
}

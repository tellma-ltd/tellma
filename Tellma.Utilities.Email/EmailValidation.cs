using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Tellma.Utilities.Email
{
    public static class EmailValidation
    {
        private static readonly EmailAddressAttribute EmailAtt = new();

        public const int MaximumEmailAddressLength = 2048;
        public const int MaximumSubjectLength = 1024;
        public const int MaximumBodyLength = 50 * 1024; // 50 KB
        public const int MaximumAttchmentSize = 2 * 1024 * 1024; // 2 MB
        public const int MaximumAttchmentNameLength = 1024;

        /// <summary>
        /// If the <see cref="EmailToSend"/> is valid, returns null, otherwise returns the validation error message.
        /// </summary>
        public static string Validate(EmailToSend email)
        {
            // Addresses

            var to = (email.To ?? new List<string>()).Where(e => !string.IsNullOrWhiteSpace(e));
            var cc = (email.Cc ?? new List<string>()).Where(e => !string.IsNullOrWhiteSpace(e));
            var bcc = (email.Bcc ?? new List<string>()).Where(e => !string.IsNullOrWhiteSpace(e));

            if (!to.Any())
            {
                return $"Missing to email address.";
            }

            if (to.Sum(e => e.Length + 1) > MaximumEmailAddressLength)
            {
                return $"Combined email To addresses exceed maximum size of {FileSizeDisplay(MaximumEmailAddressLength)}";
            }

            if (cc.Sum(e => e.Length + 1) > MaximumEmailAddressLength)
            {
                return $"Combined email Cc addresses exceed maximum size of {FileSizeDisplay(MaximumEmailAddressLength)}";
            }

            if (bcc.Sum(e => e.Length + 1) > MaximumEmailAddressLength)
            {
                return $"Combined email Bcc addresses exceed maximum size of {FileSizeDisplay(MaximumEmailAddressLength)}";
            }

            string invalidTo = to.FirstOrDefault(e => !EmailAtt.IsValid(e));
            if (invalidTo != null)
            {
                return $"Invalid To email address '{invalidTo}'.";
            }

            string invalidCc = cc.FirstOrDefault(e => !EmailAtt.IsValid(e));
            if (invalidCc != null)
            {
                return $"Invalid Cc email address '{invalidCc}'.";
            }

            string invalidBcc = bcc.FirstOrDefault(e => !EmailAtt.IsValid(e));
            if (invalidBcc != null)
            {
                return $"Invalid Bcc email address '{email.To}'.";
            }

            // Subject and Body

            if (string.IsNullOrWhiteSpace(email.Subject) && string.IsNullOrWhiteSpace(email.Body))
            {
                return $"Both subject and body are empty.";
            }

            if (email.Subject != null && email.Subject.Length > MaximumSubjectLength)
            {
                return $"Email subject exceeds maximum size of {FileSizeDisplay(MaximumSubjectLength)}";
            }

            if (email.Body != null && email.Body.Length > MaximumBodyLength)
            {
                return $"Email body exceeds the maximum size of {FileSizeDisplay(MaximumBodyLength)}.";
            }

            // Attachments

            if (email.Attachments != null)
            {
                foreach (var att in email.Attachments)
                {
                    if (string.IsNullOrWhiteSpace(att.Name))
                    {
                        return $"Missing attachment name.";
                    }

                    if (att.Name.Length > MaximumAttchmentNameLength)
                    {
                        return $"Attachment name exceeds the maximum size of {FileSizeDisplay(MaximumAttchmentNameLength)}.";
                    }

                    if (att.Contents == null || att.Contents.Length == 0)
                    {
                        return $"Missing attachment content.";
                    }
                }
            }

            return null;
        }

        private static readonly string[] Sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        private static string FileSizeDisplay(long fileSize)
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

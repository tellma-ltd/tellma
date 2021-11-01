using System.Collections.Generic;
using System.Linq;

namespace Tellma.Utilities.Email
{
    /// <summary>
    /// A DTO of an Email.
    /// </summary>
    public class EmailToSend
    {
        public EmailToSend() { }

        public EmailToSend(string toEmail) : this()
        {
            To = new List<string> { toEmail };
        }

        /// <summary>
        /// The email address to send the email to.
        /// </summary>
        public IEnumerable<string> To { get; set; } = new List<string>();

        /// <summary>
        /// The email address to send a carbon copy of the email to.
        /// </summary>
        public IEnumerable<string> Cc { get; set; } = new List<string>();

        /// <summary>
        /// The email address to send a blind carbon copy of the email to.
        /// </summary>
        public IEnumerable<string> Bcc { get; set; } = new List<string>();

        /// <summary>
        /// The subject (title) of the email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The content of the email.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The collection of files to attach to the email.
        /// </summary>
        public IEnumerable<EmailAttachmentToSend> Attachments { get; set; } = new List<EmailAttachmentToSend>();

        /// <summary>
        /// The email id in the tenant database or 0 if there is no tenant Id.
        /// </summary>
        public int EmailId { get; set; }

        /// <summary>
        /// The Id of the tenant where the email is stored or 0 if there is no tenant Id.
        /// </summary>
        public int TenantId { get; set; }
    }

    public static class EmailUtil
    {
        public static string EmailBodyBlobName(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                return null;
            }

            return $"Emails/Bodies/{guid[0..2]}/{guid[2..4]}/{guid}";
        }

        public static string EmailAttachmentBlobName(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                return null;
            }

            return $"Emails/Attachments/{guid[0..2]}/{guid[2..4]}/{guid}";
        }
    }
}

using System.Collections.Generic;

namespace Tellma.Api.Dto
{
    public class MessageCommandPreview
    {
        public string Caption { get; set; }
        public string Version { get; set; }
        public List<MessagePreview> Messages { get; set; }
    }

    public class MessagePreview
    {
        public string PhoneNumber { get; set; }
        public string Content { get; set; }
    }

    public class EmailCommandPreview
    {
        public string Caption { get; set; }
        public string Version { get; set; }
        public List<EmailPreview> Emails { get; set; }
    }

    public class EmailPreview
    {
        public string Version { get; set; }
        public List<string> To { get; set; }
        public List<string> Cc { get; set; }
        public List<string> Bcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<AttachmentPreview> Attachments { get; set; }
    }

    public class AttachmentPreview
    {
        public string DownloadName { get; set; }
        public string Body { get; set; }
    }

    public class EmailCommandVersions
    {
        /// <summary>
        /// Overall version of all emails without bodies or attachments
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Versions of individually previewed emails with body and attachments.
        /// </summary>
        public List<EmailVersion> Emails { get; set; }
    }

    public class EmailVersion
    {
        /// <summary>
        /// Index of the email.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The version of the email including body and attachments.
        /// </summary>
        public string Version { get; set; }
    }
}

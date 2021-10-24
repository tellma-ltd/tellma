using System;
using System.Collections.Generic;
using System.Text;

namespace Tellma.Api.Dto
{
    public class EmailsPreview
    {
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

    public class SmsPreview
    {
        public string To { get; set; }
        public string Body { get; set; }
    }
}

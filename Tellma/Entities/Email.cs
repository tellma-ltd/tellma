using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Email", Plural = "Emails")]
    public class Email : EntityWithKey<int>
    {
        [Display(Name = "Email_ToEmail")]
        public string ToEmail { get; set; }

        [Display(Name = "Email_FromEmail")]
        public string FromEmail { get; set; }

        [Display(Name = "Email_Subject")]
        public string Subject { get; set; }

        [Display(Name = "Email_Body")]
        public string Body { get; set; }

        // TODO: State and timestamps
    }
}

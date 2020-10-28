using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Email", Plural = "Emails")]
    public class EmailForSave : EntityWithKey<int>
    {
        [Display(Name = "Email_ToEmail")]
        public string ToEmail { get; set; }

        [Display(Name = "Email_Subject")]
        public string Subject { get; set; }

        [Display(Name = "Email_Body")]
        public string Body { get; set; }
    }

    public class EmailForQuery : EmailForSave
    {
        // TODO: State and timestamps
    }
}

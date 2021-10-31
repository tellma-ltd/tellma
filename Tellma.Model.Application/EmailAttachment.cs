using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "EmailAttachment", GroupName = "EmailAttachments")]
    public class EmailAttachmentForSave : EntityWithKey<int>
    {
        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "EmailAttachment_Content")]
        public string ContentBlobId { get; set; }
    }

    public class EmailAttachment : EmailAttachmentForSave
    {
        [Required]
        public int? EmailId { get; set; }

        [Required]
        public int? Index { get; set; }
    }
}

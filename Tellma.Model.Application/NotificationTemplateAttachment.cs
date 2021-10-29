using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class NotificationTemplateAttachmentForSave : EntityWithKey<int>
    {
        [Display(Name = "NotificationTemplate_ContextOverride")]
        [StringLength(1024)]
        public string ContextOverride { get; set; }

        [Display(Name = "NotificationTemplate_DownloadNameOverride")]
        [StringLength(1024)]
        public string DownloadNameOverride { get; set; }

        [Display(Name = "NotificationTemplate_PrintingTemplate")]
        [Required, ValidateRequired]
        public int? PrintingTemplateId { get; set; }
    }

    public class NotificationTemplateAttachment : NotificationTemplateAttachmentForSave
    {
        [Required]
        public int? NotificationTemplateId { get; set; }

        [Required]
        public int? Index { get; set; }

        // For Query

        [Display(Name = "NotificationTemplate_PrintingTemplate")]
        [ForeignKey(nameof(PrintingTemplateId))]
        public PrintingTemplate PrintingTemplate { get; set; }
    }
}

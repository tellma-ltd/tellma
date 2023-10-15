using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Attachment", GroupName = "Attachments")]
    public class ResourceAttachmentForSave : EntityWithKey<int>, IAttachment
    {
        [Display(Name = "Attachment_Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        [Required, ValidateRequired]
        public string FileName { get; set; }

        [Display(Name = "Attachment_FileExtension")]
        [StringLength(50)]
        public string FileExtension { get; set; }

        [NotMapped]
        public byte[] File { get; set; }
    }

    public class ResourceAttachment : ResourceAttachmentForSave
    {
        [Required]
        public int? ResourceId { get; set; }

        [Required]
        public string FileId { get; set; } // Ref to blob storage

        [Display(Name = "Attachment_Size")]
        [Required]
        public long Size { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "Attachment_Category")]
        [ForeignKey(nameof(CategoryId))]
        public Lookup Category { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}

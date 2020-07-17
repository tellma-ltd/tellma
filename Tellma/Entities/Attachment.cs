using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "Attachment", Plural = "Attachments")]
    public class AttachmentForSave : EntityWithKey<int>
    {
        [Display(Name = "Name")]
        [StringLength(255)]
        [Required]
        [AlwaysAccessible]
        public string FileName { get; set; }

        [Display(Name = "Attachment_FileExtension")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string FileExtension { get; set; }

        [NotMapped]
        public byte[] File { get; set; }
    }

    public class Attachment : AttachmentForSave
    {
        public int? DocumentId { get; set; }

        public string FileId { get; set; } // Ref to blob storage

        [Display(Name = "Attachment_Size")]
        [AlwaysAccessible]
        public long Size { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}

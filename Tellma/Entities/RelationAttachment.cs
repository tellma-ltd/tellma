using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "Attachment", Plural = "Attachments")]
    public class RelationAttachmentForSave : EntityWithKey<int>, IAttachment
    {
        [Display(Name = "Attachment_Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        [Required]
        [NotNull]
        [AlwaysAccessible]
        public string FileName { get; set; }

        [Display(Name = "Attachment_FileExtension")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string FileExtension { get; set; }

        [NotMapped]
        public byte[] File { get; set; }
    }

    public class RelationAttachment : RelationAttachmentForSave
    {
        [NotNull]
        public int? RelationId { get; set; }

        [NotNull]
        public string FileId { get; set; } // Ref to blob storage

        [Display(Name = "Attachment_Size")]
        [NotNull]
        [AlwaysAccessible]
        public long Size { get; set; }

        [Display(Name = "CreatedAt")]
        [NotNull]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [NotNull]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [NotNull]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
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

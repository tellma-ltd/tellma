using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    public class AttachmentForSave : EntityWithKey<int>
    {
        [Display(Name = "Name")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [AlwaysAccessible]
        public string FileName { get; set; }

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

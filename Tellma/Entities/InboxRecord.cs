using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "InboxRecord", Plural = "InboxRecords")]
    public class InboxRecord : EntityWithKey<int>
    {
        [Display(Name = "Assignment_Document")]
        public int? DocumentId { get; set; }

        [Display(Name = "Document_Comment")]
        public string Comment { get; set; }

        [Display(Name = "Document_AssignedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "Document_AssignedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "Document_OpenedAt")]
        public DateTimeOffset? OpenedAt { get; set; }

        // For Query

        [Display(Name = "Assignment_Document")]
        [ForeignKey(nameof(DocumentId))]
        public Document Document { get; set; }

        [Display(Name = "Document_AssignedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }
    }
}

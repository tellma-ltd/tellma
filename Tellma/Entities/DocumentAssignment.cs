using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "DocumentAssignment", Plural = "DocumentAssignments")]
    public class DocumentAssignment : EntityWithKey<int>
    {
        public int? DocumentId { get; set; }

        [Display(Name = "Document_Assignee")]
        [NotNull]
        public int? AssigneeId { get; set; }

        [Display(Name = "Document_Comment")]
        public string Comment { get; set; }

        [Display(Name = "Document_AssignedAt")]
        [NotNull]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [NotNull]
        public int? CreatedById { get; set; }

        [Display(Name = "Document_OpenedAt")]
        public DateTimeOffset? OpenedAt { get; set; }

        // For Query

        [Display(Name = "Document_Assignee")]
        [ForeignKey(nameof(AssigneeId))]
        public User Assignee { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }
    }
}

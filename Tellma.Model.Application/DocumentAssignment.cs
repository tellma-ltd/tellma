using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "DocumentAssignment", GroupName = "DocumentAssignments")]
    public class DocumentAssignment : EntityWithKey<int>
    {
        public int? DocumentId { get; set; }

        [Display(Name = "Document_Assignee")]
        [Required]
        public int? AssigneeId { get; set; }

        [Display(Name = "Document_Comment")]
        public string Comment { get; set; }

        [Display(Name = "Document_AssignedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

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

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "InboxRecord", GroupName = "InboxRecords")]
    public class InboxRecord : EntityWithKey<int>
    {
        [Display(Name = "Assignment_Document")]
        [Required]
        public int? DocumentId { get; set; }

        [Display(Name = "Document_Comment")]
        public string Comment { get; set; }

        [Display(Name = "Document_AssignedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "Document_AssignedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "Document_Assignee")]
        [Required]
        public int? AssigneeId { get; set; }

        [Display(Name = "Document_OpenedAt")]
        public DateTimeOffset? OpenedAt { get; set; }

        // For Query

        [Display(Name = "Assignment_Document")]
        [ForeignKey(nameof(DocumentId))]
        public Document Document { get; set; }

        [Display(Name = "Document_AssignedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "Document_Assignee")]
        [ForeignKey(nameof(AssigneeId))]
        public User Assignee { get; set; }
    }
}

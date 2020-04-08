using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    public class OutboxRecord : EntityWithKey<int>
    {
        [Display(Name = "Assignment_Document")]
        public int? DocumentId { get; set; }

        [Display(Name = "Document_Comment")]
        public string Comment { get; set; }

        [Display(Name = "Document_AssignedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "Document_Assignee")]
        public int? AssigneeId { get; set; }

        [Display(Name = "Document_OpenedAt")]
        public DateTimeOffset? OpenedAt { get; set; }

        // For Query

        [Display(Name = "Assignment_Document")]
        [ForeignKey(nameof(DocumentId))]
        public Document Document { get; set; }

        [Display(Name = "Document_Assignee")]
        [ForeignKey(nameof(AssigneeId))]
        public User Assignee { get; set; }
    }
}

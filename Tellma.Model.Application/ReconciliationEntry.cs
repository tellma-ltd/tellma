using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class ReconciliationEntryForSave : EntityWithKey<int>
    {
        [Required, ValidateRequired]
        public int? EntryId { get; set; }
    }

    public class ReconciliationEntry : ReconciliationEntryForSave
    {
        public int? ReconciliationId { get; set; }

        [ForeignKey(nameof(EntryId))]
        public EntryForReconciliation Entry { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class ReconciliationExternalEntryForSave : EntityWithKey<int>
    {
        // TODO: Should this be required?
        public int? ExternalEntryId { get; set; }

        [NotMapped]
        public int? ExternalEntryIndex { get; set; }
    }

    public class ReconciliationExternalEntry : ReconciliationExternalEntryForSave
    {
        public int? ReconciliationId { get; set; }

        [ForeignKey(nameof(ExternalEntryId))]
        public ExternalEntry ExternalEntry { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }
    }
}

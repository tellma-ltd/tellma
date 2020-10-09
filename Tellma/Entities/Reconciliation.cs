using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Entities
{
    public class ReconciliationForSave<TEntry, TExternalEntry> : EntityWithKey<int>
    {
        public List<TEntry> Entries { get; set; }
        public List<TExternalEntry> ExternalEntries { get; set; }
    }

    public class ReconciliationForSave : ReconciliationForSave<ReconciliationEntryForSave, ReconciliationExternalEntryForSave>
    {
    }

    public class Reconciliation : ReconciliationForSave<ReconciliationEntry, ReconciliationExternalEntry>
    {

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }
    }

    public class ReconciliationEntryForSave : EntityWithKey<int>
    {
        [Required]
        public int? EntryId { get; set; }
    }

    public class ReconciliationEntry : ReconciliationEntryForSave
    {
        [ForeignKey(nameof(EntryId))]
        public Entry Entry { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }
    }

    public class ReconciliationExternalEntryForSave : EntityWithKey<int>
    {
        public int? ExternalEntryId { get; set; }

        [NotMapped]
        public int? ExternalEntryIndex { get; set; }
    }

    public class ReconciliationExternalEntry : ReconciliationExternalEntryForSave
    {
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

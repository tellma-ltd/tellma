using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class ReconciliationForSave<TEntry, TExternalEntry> : EntityWithKey<int>
    {
        [ForeignKey(nameof(ReconciliationEntry.ReconciliationId))]
        public List<TEntry> Entries { get; set; }

        [ForeignKey(nameof(ReconciliationExternalEntry.ReconciliationId))]
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
}

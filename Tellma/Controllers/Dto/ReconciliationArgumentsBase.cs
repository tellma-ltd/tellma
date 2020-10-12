using System;
using System.Collections.Generic;
using Tellma.Entities;

namespace Tellma.Controllers.Dto
{
    public class ReconciliationArgumentsBase
    {
        public int AccountId { get; set; }
        public int CustodyId { get; set; }
    }

    public class ReconciliationGetUnreconciledArguments : ReconciliationArgumentsBase
    {
        public DateTime? AsOfDate { get; set; } = DateTime.Today;
        public int EntriesTop { get; set; }
        public int EntriesSkip { get; set; }
        public int ExternalEntriesTop { get; set; }
        public int ExternalEntriesSkip { get; set; }
    }

    public class ReconciliationGetReconciledArguments : ReconciliationArgumentsBase
    {
        public DateTime? FromDate { get; set; } = new DateTime(1800, 1, 1);
        public DateTime? ToDate { get; set; } = DateTime.Today;
        public decimal? FromAmount { get; set; }
        public decimal? ToAmount { get; set; }
        public string ExternalReferenceContains { get; set; }
        public int Top { get; set; }
        public int Skip { get; set; }
    }

    public class ReconciliationSavePayload
    {
        public List<ExternalEntryForSave> ExternalEntries { get; set; }
        public List<ReconciliationForSave> Reconciliations { get; set; }
        public List<int> DeletedExternalEntryIds { get; set; }
        public List<int> DeletedReconciliationIds { get; set; }
    }

    public class ReconciliationGetUnreconciledResponse
    {
        public List<ExternalEntry> ExternalEntries { get; set; }
        public List<EntryForReconciliation> Entries { get; set; }
        public decimal EntriesBalance { get; set; }
        public decimal UnreconciledEntriesBalance { get; set; }
        public decimal UnreconciledExternalEntriesBalance { get; set; }
        public int UnreconciledEntriesCount { get; set; }
        public int UnreconciledExternalEntriesCount { get; set; }
    }

    public class ReconciliationGetReconciledResponse
    {
        public List<Reconciliation> Reconciliations { get; set; }
        public int ReconciledCount { get; set; }
    }
}

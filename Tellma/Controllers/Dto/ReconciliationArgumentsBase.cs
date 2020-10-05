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

    public class ReconciliationLoadUnreconciledArguments : ReconciliationArgumentsBase
    {
        public DateTime AsOfDate { get; set; }
        public int EntriesTop { get; set; }
        public int EntriesSkip { get; set; }
        public int ExternalEntriesTop { get; set; }
        public int ExternalEntriesSkip { get; set; }
    }

    public class ReconciliationLoadReconciledArguments : ReconciliationArgumentsBase
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? FromAmount { get; set; }
        public decimal? ToAmount { get; set; }
        public string ExternalReferenceContains { get; set; }
        public int Top { get; set; }
        public int Skip { get; set; }
    }

    public class ReconciliationSaveArguments : ReconciliationArgumentsBase
    {
        public bool ReturnReconciled { get; set; }
        public DateTime? AsOfDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
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

    public class ReconciliationLoadUnreconciledResponse
    {
        public List<ExternalEntry> ExternalEntries { get; set; }
        public List<Entry> Entries { get; set; }
    }

    public class ReconciliationLoadReconciledResponse : ReconciliationLoadUnreconciledResponse
    {
        public List<Reconciliation> Reconciliations { get; set; }
    }
}

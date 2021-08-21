using System.Collections.Generic;
using Tellma.Model.Application;

namespace Tellma.Api.Dto
{
    public class ReconciliationGetUnreconciledResponse
    {
        public IEnumerable<ExternalEntry> ExternalEntries { get; set; }
        public IEnumerable<EntryForReconciliation> Entries { get; set; }
        public decimal EntriesBalance { get; set; }
        public decimal UnreconciledEntriesBalance { get; set; }
        public decimal UnreconciledExternalEntriesBalance { get; set; }
        public int UnreconciledEntriesCount { get; set; }
        public int UnreconciledExternalEntriesCount { get; set; }
    }
}

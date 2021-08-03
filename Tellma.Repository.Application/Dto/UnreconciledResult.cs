using System.Collections.Generic;
using Tellma.Model.Application;

namespace Tellma.Repository.Application
{
    public class UnreconciledResult
    {
        public UnreconciledResult(
            decimal entriesBalance,
            decimal unreconciledEntriesBalance,
            decimal unreconciledExternalEntriesBalance,
            int unreconciledEntriesCount,
            int unreconciledExternalEntriesCount,
            List<EntryForReconciliation> entries,
            List<ExternalEntry> externalEntries
            )
        {
            EntriesBalance = entriesBalance;
            UnreconciledEntriesBalance = unreconciledEntriesBalance;
            UnreconciledExternalEntriesBalance = unreconciledExternalEntriesBalance;
            UnreconciledEntriesCount = unreconciledEntriesCount;
            UnreconciledExternalEntriesCount = unreconciledExternalEntriesCount;
            Entries = entries;
            ExternalEntries = externalEntries;
        }

        public decimal EntriesBalance { get; }
        public decimal UnreconciledEntriesBalance { get; }
        public decimal UnreconciledExternalEntriesBalance { get; }
        public int UnreconciledEntriesCount { get; }
        public int UnreconciledExternalEntriesCount { get; }
        public List<EntryForReconciliation> Entries { get; }
        public List<ExternalEntry> ExternalEntries { get; }
    }
}

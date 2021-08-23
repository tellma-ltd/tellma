using System;

namespace Tellma.Api.Dto
{
    public class ReconciliationGetUnreconciledArguments : ReconciliationArgumentsBase
    {
        public DateTime? AsOfDate { get; set; } = DateTime.Today;
        public int EntriesTop { get; set; }
        public int EntriesSkip { get; set; }
        public int ExternalEntriesTop { get; set; }
        public int ExternalEntriesSkip { get; set; }
    }
}

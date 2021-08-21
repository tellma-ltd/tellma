using System;

namespace Tellma.Api.Dto
{
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
}

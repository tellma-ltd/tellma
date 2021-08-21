using System.Collections.Generic;
using Tellma.Model.Application;

namespace Tellma.Api.Dto
{
    public class ReconciliationGetReconciledResponse
    {
        public IEnumerable<Reconciliation> Reconciliations { get; set; }
        public int ReconciledCount { get; set; }
    }
}

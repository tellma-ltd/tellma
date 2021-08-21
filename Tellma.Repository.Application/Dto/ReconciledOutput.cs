using System.Collections.Generic;
using Tellma.Model.Application;

namespace Tellma.Repository.Application
{
    public class ReconciledOutput
    {
        public ReconciledOutput(int reconciledCount, List<Reconciliation> reconciliations)
        {
            ReconciledCount = reconciledCount;
            Reconciliations = reconciliations;
        }

        public int ReconciledCount { get; }
        public List<Reconciliation> Reconciliations { get; }
    }
}

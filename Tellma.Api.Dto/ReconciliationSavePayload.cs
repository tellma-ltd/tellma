using System.Collections.Generic;
using Tellma.Model.Application;

namespace Tellma.Api.Dto
{
    public class ReconciliationSavePayload
    {
        public List<ExternalEntryForSave> ExternalEntries { get; set; }
        public List<ReconciliationForSave> Reconciliations { get; set; }
        public List<int> DeletedExternalEntryIds { get; set; }
        public List<int> DeletedReconciliationIds { get; set; }
    }
}

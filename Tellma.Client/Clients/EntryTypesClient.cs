using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class EntryTypesClient : CrudClientBase<EntryTypeForSave, EntryType, int>
    {
        internal EntryTypesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "entry-types";

        public async Task<EntitiesResult<EntryType>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<EntryType>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }
}

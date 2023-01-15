using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class AccountClassificationsClient : CrudClientBase<AccountClassificationForSave, AccountClassification, int>
    {
        internal AccountClassificationsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "account-classifications";

        public async Task<EntitiesResult<AccountClassification>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<AccountClassification>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }
}

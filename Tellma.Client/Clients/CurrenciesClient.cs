using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class CurrenciesClient : CrudClientBase<CurrencyForSave, Currency, string>
    {
        internal CurrenciesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "currencies";

        public async Task<EntitiesResult<Currency>> Activate(List<string> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<Currency>> Deactivate(List<string> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }
}

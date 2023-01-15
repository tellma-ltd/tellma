using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Admin;

namespace Tellma.Client
{
    public class AdminUsersClient : CrudClientBase<AdminUserForSave, AdminUser, int>
    {
        internal AdminUsersClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "admin-users";

        public async Task<EntitiesResult<AdminUser>> Activate(List<int> ids, Request<ActivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await ActivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }

        public async Task<EntitiesResult<AdminUser>> Deactivate(List<int> ids, Request<DeactivateArguments> request = null, CancellationToken cancellation = default)
        {
            return await DeactivateImpl(ids, request, cancellation).ConfigureAwait(false);
        }
    }
}

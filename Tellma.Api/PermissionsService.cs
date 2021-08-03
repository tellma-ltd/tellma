using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;

namespace Tellma.Api
{
    public class PermissionsService : ServiceBase
    {
        private readonly ApplicationServiceBehavior _behavior;
        private readonly IPermissionsCache _permissionsCache;

        public PermissionsService(
            ApplicationServiceBehavior behavior, 
            IServiceContextAccessor accessor, 
            IPermissionsCache permissionsCache) : base(accessor)
        {
            _behavior = behavior;
            _permissionsCache = permissionsCache;
        }

        protected override IServiceBehavior Behavior => _behavior;

        public async Task<Versioned<PermissionsForClient>> PermissionsForClient(CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _permissionsCache.GetPermissionss(UserId, _behavior.TenantId, _behavior.PermissionsVersion, cancellation);
        }
    }
}

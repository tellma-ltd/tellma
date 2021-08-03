using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Repository.Admin;

namespace Tellma.Api
{
    public class AdminPermissionsService : ServiceBase
    {
        private readonly AdminServiceBehavior _behavior;
        private readonly AdminRepository _repo;

        public AdminPermissionsService(
            AdminServiceBehavior behavior, 
            IServiceContextAccessor accessor, 
            AdminRepository repo) : base(accessor)
        {
            _behavior = behavior;
            _repo = repo;
        }

        protected override IServiceBehavior Behavior => _behavior;

        public virtual async Task<Versioned<AdminPermissionsForClient>> PermissionsForClient(CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Retrieve the user permissions and their current version
            var (version, permissions) = await _repo.Permissions__Load(UserId, cancellation);

            // Arrange the permission in a DTO that is easy for clients to consume
            var permissionsForClient = new AdminPermissionsForClient
            {
                Permissions = permissions.Select(p => new UserPermission
                {
                    Action = p.Action,
                    Criteria = p.Criteria,
                    View = p.View
                })
            };

            // Tag the permissions for client with their current version
            var result = new Versioned<AdminPermissionsForClient>
            (
                version: version.ToString(),
                data: permissionsForClient
            );

            // Return the result
            return result;
        }
    }
}

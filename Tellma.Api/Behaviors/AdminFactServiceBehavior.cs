using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Model.Common;
using Tellma.Repository.Admin;
using Tellma.Repository.Common;

namespace Tellma.Api.Behaviors
{
    public class AdminFactServiceBehavior : AdminServiceBehavior, IFactServiceBehavior
    {
        public AdminFactServiceBehavior(
            IServiceContextAccessor context, 
            AdminRepository adminRepo, 
            AdminVersions versions,
            ILogger<AdminServiceBehavior> logger) : base(context, adminRepo, versions, logger)
        {
        }

        public void SetDefinitionId(int definitionId)
        {
            // No Definitions in admin
        }

        public IQueryFactory QueryFactory<TEntity>() where TEntity : Entity
        {
            return Repository;
        }

        public async Task<IEnumerable<AbstractPermission>> UserPermissions(string view, string action, CancellationToken cancellation)
        {
            // It would be an overkill to cache permissions for admin users
            return await Repository.Action_View__Permissions(UserId, action, view, cancellation);
        }
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Model.Common;
using Tellma.Repository.Common;
using Tellma.Repository.Identity;

namespace Tellma.Services.EmbeddedIdentityServer
{
    public class IdentityFactServiceBehavior : IFactServiceBehavior
    {
        private readonly AdminFactServiceBehavior _adminBehavior;
        private readonly IdentityRepository _idRepo;

        public IdentityFactServiceBehavior(AdminFactServiceBehavior adminBehavior, IdentityRepository idRepo)
        {
            _adminBehavior = adminBehavior;
            _idRepo = idRepo;
        }

        public Task<int> OnInitialize(CancellationToken cancellation)
        {
            return _adminBehavior.OnInitialize(cancellation);
        }

        public IQueryFactory QueryFactory<TEntity>() where TEntity : Entity
        {
            // This is the only place where identity behavior differs from admin behavior
            return _idRepo;
        }

        public void SetDefinitionId(int definitionId)
        {
        }

        public Task<IEnumerable<AbstractPermission>> UserPermissions(string view, string action, CancellationToken cancellation)
        {
            return _adminBehavior.UserPermissions(view, action, cancellation);
        }
    }
}

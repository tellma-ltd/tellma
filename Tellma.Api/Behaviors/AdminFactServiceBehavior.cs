using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Metadata;
using Tellma.Api.Templating;
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

        #region Markup Templates

        public Task<AbstractMarkupTemplate> GetMarkupTemplate<TEntity>(int templateId, CancellationToken cancellation) where TEntity : Entity
        {
            throw new ServiceException("Markup templates are not supported in admin Fact APIs.");
        }

        public Task SetMarkupFunctions(Dictionary<string, EvaluationFunction> localVariables, Dictionary<string, EvaluationFunction> globalVariables, CancellationToken cancellation)
        {
            return Task.CompletedTask; // No markup functions
        }

        public Task SetMarkupVariables(Dictionary<string, EvaluationVariable> localVariables, Dictionary<string, EvaluationVariable> globalVariables, CancellationToken cancellation)
        {
            return Task.CompletedTask; // No markup variables
        }

        #endregion

        public Task<IMetadataOverridesProvider> GetMetadataOverridesProvider(CancellationToken cancellation)
        {
            // Nothing to override
            var nullOverrides = new NullMetadataOverridesProvider();
            return Task.FromResult<IMetadataOverridesProvider>(nullOverrides);
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

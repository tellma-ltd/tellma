using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;

namespace Tellma.Api
{
    public class DefinitionsService : ServiceBase
    {
        private readonly ApplicationServiceBehavior _behavior;
        private readonly IDefinitionsCache _definitionsCache;

        public DefinitionsService(
            ApplicationServiceBehavior behavior, 
            IServiceContextAccessor contextAccessor,
            IDefinitionsCache definitionsCache) : base(contextAccessor)
        {
            _behavior = behavior;
            _definitionsCache = definitionsCache;
        }

        protected override IServiceBehavior Behavior => _behavior;

        public async Task<Versioned<DefinitionsForClient>> DefinitionsForClient(CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _definitionsCache.GetDefinitions(_behavior.TenantId, _behavior.DefinitionsVersion, cancellation);
        }
    }
}

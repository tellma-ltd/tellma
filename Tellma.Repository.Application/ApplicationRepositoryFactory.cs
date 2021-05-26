using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Tellma.Utilities.Sharding;

namespace Tellma.Repository.Application
{
    public class ApplicationRepositoryFactory : IApplicationRepositoryFactory
    {
        private readonly ILogger<ApplicationRepository> _logger;
        private readonly ICachingShardResolver _shardResolver;
        private readonly ConcurrentDictionary<int, ApplicationRepository> _repos = new();

        public ApplicationRepositoryFactory(ILogger<ApplicationRepository> logger, ICachingShardResolver shardResolver)
        {
            _logger = logger;
            _shardResolver = shardResolver;
        }

        public ApplicationRepository GetRepository(int tenantId)
        {
            return _repos.GetOrAdd(tenantId, 
                tenantId => new ApplicationRepository(tenantId, _shardResolver, _logger));
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Utilities.Caching;

namespace Tellma.Api
{
    public class SettingsCache : VersionCache<int, SettingsForClient>
    {
        private readonly ApplicationRepositoryFactory _repoFactory;

        public SettingsCache(ApplicationRepositoryFactory repoFactory)
        {
            _repoFactory = repoFactory;
        }

        protected override Task<(SettingsForClient data, string version)> GetDataFromSource(int tenantId, CancellationToken cancellation)
        {
            var repo = _repoFactory.GetRepository(tenantId);
            repo.
        }

        public async Task<Versioned<SettingsForClient>> GetSettings(int tenantId, string version, CancellationToken cancellation)
        {
            var (data, newVersion) = await GetData(tenantId, version, cancellation);
            return new Versioned<SettingsForClient>(data, newVersion);
        }
    }
}

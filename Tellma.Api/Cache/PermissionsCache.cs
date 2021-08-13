using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Repository.Application;
using Tellma.Utilities.Caching;

namespace Tellma.Api
{
    internal class PermissionsCache : VersionCache<(int, int), PermissionsForClient>, IPermissionsCache
    {
        private readonly IApplicationRepositoryFactory _repoFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionsCache"/> class.
        /// </summary>
        public PermissionsCache(IApplicationRepositoryFactory repoFactory)
        {
            _repoFactory = repoFactory;
        }

        /// <summary>
        /// Implementation of <see cref="VersionCache{TKey, TData}"/>.
        /// </summary>
        protected override async Task<(PermissionsForClient data, string version)> GetDataFromSource((int, int) key, CancellationToken cancellation)
        {
            var (userId, tenantId) = key;
            var repo = _repoFactory.GetRepository(tenantId);

            PermissionsResult permissionsResult = await repo.Permissions__Load(userId, cancellation);

            var version = permissionsResult.Version.ToString();
            var permissions = permissionsResult.Permissions
                .Select(e => new UserPermission
                {
                    View = e.View,
                    Action = e.Action,
                    Criteria = e.Criteria
                });
            var reportIds = permissionsResult.ReportIds;
            var dashboardIds = permissionsResult.DashboardIds;

            var forClient = new PermissionsForClient
            {
                Permissions = permissions,
                ReportIds = reportIds,
                DashboardIds = dashboardIds
            };

            return (forClient, version);
        }

        /// <summary>
        /// Returns the user permissions in a specific company from the cache if <paramref name="version"/> matches 
        /// the cached version, otherwise retrieves the permissions from the company's database.
        /// <para/>
        /// Note: The calling service has to retrieve the <paramref name="version"/> independently using 
        /// <see cref="ApplicationRepository.OnConnect"/>, all services already do that to retrieve the 
        /// user Id so they retrieve the <paramref name="version"/> in the same database call as a performance optimization.
        /// </summary>
        /// <param name="userId">The ID of the user whose permissions to load.</param>
        /// <param name="tenantId">The ID of the company database where the user permissions are found.</param>
        /// <param name="version">The latest version of the user permissions in the specific company.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The user's permissions in the specific company packaged in a <see cref="PermissionsForClient"/> object, together with their version.</returns>
        public async Task<Versioned<PermissionsForClient>> GetPermissionss(int userId, int tenantId, string version, CancellationToken cancellation = default)
        {
            var (data, newVersion) = await GetData((userId, tenantId), version, cancellation);
            return new Versioned<PermissionsForClient>(data, newVersion);
        }
    }
}

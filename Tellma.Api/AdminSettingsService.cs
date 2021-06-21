using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Repository.Admin;

namespace Tellma.Api
{
    public class AdminSettingsService : ServiceBase
    {
        private readonly AdminServiceBehavior _behavior;
        private readonly AdminRepository _repo;

        public AdminSettingsService(
            AdminServiceBehavior behavior, 
            AdminRepository repo, 
            IServiceContextAccessor accessor) : base (accessor)
        {
            _behavior = behavior;
            _repo = repo;
        }

        protected override IServiceBehavior Behavior => _behavior;

        public async Task Ping(CancellationToken cancellation)
        {
            // The sole purpose of this API is to retrieve the latest cache versions and check their freshness
            await Initialize(cancellation);
        }

        public async Task<Versioned<AdminSettingsForClient>> SettingsForClient(CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Simply retrieves the cached settings, which were refreshed by AdminApiAttribute
            var adminSettings = await _repo.Settings__Load(cancellation);
            if (adminSettings == null)
            {
                throw new ServiceException("Admin Settings were not initialized.");
            }

            var adminSettingsForClient = new AdminSettingsForClient
            {
                CreatedAt = adminSettings.CreatedAt
            };

            var result = new Versioned<AdminSettingsForClient>
            (
                data: adminSettingsForClient,
                version: adminSettings.SettingsVersion.ToString()
            );

            return result;
        }
    }
}

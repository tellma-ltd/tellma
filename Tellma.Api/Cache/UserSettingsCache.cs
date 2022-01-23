using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Repository.Application;
using Tellma.Utilities.Caching;

namespace Tellma.Api
{
    internal class UserSettingsCache : VersionCache<(int userId, int tenantId), UserSettingsForClient>, IUserSettingsCache
    {
        private readonly IApplicationRepositoryFactory _repoFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettingsCache"/> class.
        /// </summary>
        public UserSettingsCache(IApplicationRepositoryFactory repoFactory)
        {
            _repoFactory = repoFactory;
        }

        /// <summary>
        /// Implementation of <see cref="VersionCache{TKey, TData}"/>.
        /// </summary>
        protected override async Task<(UserSettingsForClient data, string version)> GetDataFromSource((int userId, int tenantId) key, CancellationToken cancellation)
        {
            var (userId, tenantId) = key;
            var repo = _repoFactory.GetRepository(tenantId);

            UserSettingsOutput usResult = await repo.UserSettings__Load(userId, cancellation);

            var version = usResult.Version.ToString();
            var user = usResult.User;
            var customSettings = usResult.CustomSettings;

            // prepare the result
            var forClient = new UserSettingsForClient
            {
                UserId = user.Id,
                Name = user.Name,
                Name2 = user.Name2,
                Name3 = user.Name3,
                Email = user.Email,
                ImageId = user.ImageId,
                PreferredLanguage = user.PreferredLanguage,
                PreferredCalendar = user.PreferredCalendar,
                CustomSettings = customSettings.ToDictionary(e => e.Key, e => e.Value),
            };

            return (forClient, version);
        }

        /// <summary>
        /// Returns the user settings in a specific company from the cache if <paramref name="version"/> matches 
        /// the cached version, otherwise retrieves the user settings from the company's database.
        /// <para/>
        /// Note: The calling service has to retrieve the <paramref name="version"/> independently using 
        /// <see cref="ApplicationRepository.OnConnect"/>, all services already do that to retrieve the 
        /// user Id so they retrieve the <paramref name="version"/> in the same database call as a performance optimization.
        /// </summary>
        /// <param name="userId">The ID of the user whose settings to load.</param>
        /// <param name="tenantId">The ID of the company database where the user settings are found.</param>
        /// <param name="version">The latest version of the user settings in the specific company.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The user settings in the specific company packaged in a <see cref="UserSettingsForClient"/> object, together with their version.</returns>
        public async Task<Versioned<UserSettingsForClient>> GetUserSettings(int userId, int tenantId, string version, CancellationToken cancellation = default)
        {
            var (data, newVersion) = await GetData((userId, tenantId), version, cancellation);
            return new Versioned<UserSettingsForClient>(data, newVersion);
        }
    }
}

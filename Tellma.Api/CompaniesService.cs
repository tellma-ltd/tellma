using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Repository.Admin;
using Tellma.Repository.Application;

namespace Tellma.Api
{
    public class CompaniesService : ServiceBase
    {
        private readonly AdminRepository _adminRepo;
        private readonly IApplicationRepositoryFactory _factory;
        private readonly ISettingsCache _settingsCache;
        private readonly ILogger _logger;
        private readonly NullServiceBehavior _behavior;

        protected override IServiceBehavior Behavior => _behavior;

        public CompaniesService(
            IServiceContextAccessor accessor,
            AdminRepository db,
            IApplicationRepositoryFactory factory,
            ISettingsCache settingsCache,
            NullServiceBehavior behavior,
            ILogger<CompaniesService> logger) : base (accessor)
        {
            _adminRepo = db;
            _factory = factory;
            _settingsCache = settingsCache;
            _logger = logger;
            _behavior = behavior;
        }

        public async Task<CompaniesForClient> GetForClient(CancellationToken cancellation)
        {
            var companies = new ConcurrentBag<UserCompany>();
            var (databaseIds, isAdmin) = await _adminRepo.GetAccessibleDatabaseIds(ExternalUserId, ExternalEmail, cancellation);

            // Connect all the databases in parallel and make sure the user can access them all
            await Task.WhenAll(databaseIds.Select(async (databaseId) =>
            {
                try
                {
                    var appRepo = _factory.GetRepository(databaseId);

                    var result = await appRepo.OnConnect(ExternalUserId, ExternalEmail, setLastActive: false, cancellation);
                    if (result.UserId != null)
                    {
                        var settingsVersion = result.SettingsVersion.ToString();
                        var settings = (await _settingsCache.GetSettings(databaseId, settingsVersion, cancellation)).Data;

                        companies.Add(new UserCompany
                        {
                            Id = databaseId,
                            Name = settings.ShortCompanyName,
                            Name2 = Normalize(settings.SecondaryLanguageId),
                            Name3 = Normalize(settings.TernaryLanguageId)
                        });
                    }
                }
                catch (Exception ex)
                {
                    // If we fail to connect to a company, this company simply isn't added to the result
                    _logger.LogWarning(ex, $"Exception while connecting to user company: DatabaseId: {databaseId}, User email: {ExternalEmail}.");
                }
            }));

            // Confirm isAdmin by checking with the admin DB
            if (isAdmin)
            {
                var result = await _adminRepo.OnConnect(ExternalUserId, ExternalEmail, cancellation);
                isAdmin = result?.UserId != null;
            }

            return new CompaniesForClient
            {
                IsAdmin = isAdmin,
                Companies = companies.OrderBy(e => e.Id).ToList(),
            };
        }

        /// <summary>
        /// Turns empty or white space strings into nulls.
        /// </summary>
        private static string Normalize(string str) => string.IsNullOrWhiteSpace(str) ? null : str;
    }
}

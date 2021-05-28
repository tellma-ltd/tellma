using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tellma.Repository.Admin;
using Tellma.Repository.Application;

namespace Tellma.Controllers
{
    public class ApplicationServiceInitializer : IServiceInitializer
    {
        private readonly IApplicationRepositoryFactory _repositoryFactory;
        private readonly AdminRepository _adminRepo;
        private readonly ILogger _logger;

        public ApplicationServiceInitializer(
            IApplicationRepositoryFactory repositoryFactory, 
            AdminRepository adminRepo, 
            ILogger<ApplicationServiceInitializer> logger)
        {
            _repositoryFactory = repositoryFactory;
            _adminRepo = adminRepo;
            _logger = logger;
        }

        private bool _isInitialized = false;
        private string _settingsVersion;
        private string _definitionsVersion;
        private string _userSettingsVersion;
        private string _permissionsVersion;
        private ApplicationRepository _appRepo;

        public bool IsInitialized => _isInitialized;

        public string SettingsVersion => IsInitialized ? _settingsVersion : 
            throw new InvalidOperationException($"Accessing {nameof(SettingsVersion)} before initializing the service.");
        public string DefinitionsVersion => IsInitialized ? _definitionsVersion : 
            throw new InvalidOperationException($"Accessing {nameof(DefinitionsVersion)} before initializing the service.");
        public string UserSettingsVersion => IsInitialized ? _userSettingsVersion : 
            throw new InvalidOperationException($"Accessing {nameof(UserSettingsVersion)} before initializing the service.");
        public string PermissionsVersion => IsInitialized ? _permissionsVersion :
            throw new InvalidOperationException($"Accessing {nameof(PermissionsVersion)} before initializing the service.");
        public ApplicationRepository Repository => IsInitialized ? _appRepo :
            throw new InvalidOperationException($"Accessing {nameof(Repository)} before initializing the service.");

        public async Task<int> OnInitialize(ServiceContext ctx)
        {
            // (1) Make sure the API caller have provided a tenantId, and extract it
            var tenantId = ctx.TenantId ?? throw new ServiceException($"TenantId was not supplied.");

            // Extract the relevant context information
            var ctxExternalId = ctx.ExternalUserId;
            var ctxEmail = ctx.ExternalEmail;
            var setLastActive = !ctx.IsSilent;
            var cancellation = ctx.Cancellation;

            // (2) Call OnConnect...
            // The client sometimes makes ambient API calls, not in response to user interaction
            // Such calls should not update LastAccess of that user
            _appRepo = _repositoryFactory.GetRepository(tenantId);
            var result = await _appRepo.OnConnect(ctxExternalId, ctxEmail, setLastActive, cancellation);

            // (3) Make sure the user is a member of this tenant
            if (result.UserId == null)
            {
                throw new ForbiddenException(notMember: true);
            }

            // Extract values from the result
            var userId = result.UserId.Value;
            var dbExternalId = result.ExternalId;
            var dbEmail = result.Email;

            // (4) If the user exists but new, set the External Id
            if (dbExternalId == null)
            {
                // Update external Id in this tenant database
                await _appRepo.Users__SetExternalIdByUserId(userId, ctxExternalId);

                // Update external Id in the central Admin database too (To avoid an awkward situation
                // where a user exists on the tenant but not on the Admin db, if they change their email in between)
                await _adminRepo.DirectoryUsers__SetExternalIdByEmail(ctxEmail, ctxExternalId);
            }
            else if (dbExternalId != ctxExternalId)
            {
                // Note: there is the edge case of identity providers who allow email recycling. I.e. we can get the same email twice with 
                // two different external Ids. This issue is so unlikely to naturally occur and cause problems here that we are not going
                // to handle it for now. It can however happen artificually if the application is re-configured to a new identity provider,
                // or if someone messed with the identity database directly, but again out of scope for now.
                throw new InvalidOperationException($"The sign-in email '{dbEmail}' already exists but with a different external Id. TenantId: {tenantId}.");
            }

            // (5) If the user's email address has changed at the identity server, update it locally
            else if (dbEmail != ctxEmail)
            {
                await _appRepo.Users__SetEmailByUserId(userId, ctxEmail);
                await _adminRepo.DirectoryUsers__SetEmailByExternalId(ctxExternalId, ctxEmail);
                _logger.LogWarning($"A user's email has been updated from '{dbEmail}' to '{ctxEmail}'. TenantId: {tenantId}.");
            }

            // (6) Mark this initializer as initialized and set the versions.
            _isInitialized = true;
            _settingsVersion = result.SettingsVersion.ToString();
            _definitionsVersion = result.DefinitionsVersion.ToString();
            _userSettingsVersion = result.UserSettingsVersion?.ToString();
            _permissionsVersion = result.PermissionsVersion?.ToString();

            // (7) Return the user Id 
            return userId;
        }
    }
}

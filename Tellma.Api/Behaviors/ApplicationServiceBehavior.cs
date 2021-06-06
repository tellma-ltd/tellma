using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Repository.Admin;
using Tellma.Repository.Application;

namespace Tellma.Api.Behaviors
{
    public class ApplicationServiceBehavior : IServiceBehavior
    {
        private readonly IApplicationRepositoryFactory _repositoryFactory;
        private readonly ApplicationVersions _versions;
        private readonly AdminRepository _adminRepo;
        private readonly ILogger _logger;

        private readonly string _externalId;
        private readonly string _externalEmail;
        private readonly int _tenantId;
        private readonly ApplicationRepository _appRepo;
        private readonly bool _isSilent;

        public ApplicationServiceBehavior(
            IServiceContextAccessor context,
            IApplicationRepositoryFactory repositoryFactory, 
            ApplicationVersions versions,
            AdminRepository adminRepo, 
            ILogger<ApplicationServiceBehavior> logger)
        {
            _repositoryFactory = repositoryFactory;
            _versions = versions;
            _adminRepo = adminRepo;
            _logger = logger;

            // Extract information from the Context Accessor
            _externalId = context.ExternalUserId ?? throw new ServiceException($"External user id was not supplied.");
            _externalEmail = context.ExternalEmail ?? throw new ServiceException($"External user email was not supplied.");
            _tenantId = context.TenantId ?? throw new ServiceException($"Tenant id was not supplied.");
            _appRepo = _repositoryFactory.GetRepository(_tenantId);
            _isSilent = context.IsSilent;
        }

        private string _userEmail;
        private int _userId;

        public bool IsInitialized { get; private set; } = false;

        public string SettingsVersion => IsInitialized ? _versions.SettingsVersion : 
            throw new InvalidOperationException($"Accessing {nameof(SettingsVersion)} before initializing the service.");
        public string DefinitionsVersion => IsInitialized ? _versions.DefinitionsVersion : 
            throw new InvalidOperationException($"Accessing {nameof(DefinitionsVersion)} before initializing the service.");
        public string UserSettingsVersion => IsInitialized ? _versions.UserSettingsVersion : 
            throw new InvalidOperationException($"Accessing {nameof(UserSettingsVersion)} before initializing the service.");
        public string PermissionsVersion => IsInitialized ? _versions.PermissionsVersion :
            throw new InvalidOperationException($"Accessing {nameof(PermissionsVersion)} before initializing the service.");

        protected string UserEmail => IsInitialized ? _userEmail :
            throw new InvalidOperationException($"Accessing {nameof(UserEmail)} before initializing the service.");
        protected int UserId => IsInitialized ? _userId :
            throw new InvalidOperationException($"Accessing {nameof(UserId)} before initializing the service.");

        public virtual async Task<int> OnInitialize(CancellationToken cancellation)
        {
            // (1) Call OnConnect...
            // The client sometimes makes ambient API calls, not in response to user interaction
            // Such calls should not update LastAccess of that user
            var result = await _appRepo.OnConnect(_externalId, _externalEmail, setLastActive: !_isSilent, cancellation);

            // (2) Make sure the user is a member of this tenant
            if (result.UserId == null)
            {
                // Not a member
                throw new ForbiddenException(notMember: true);
            }

            // Extract values from the result
            var userId = result.UserId.Value;
            var dbExternalId = result.ExternalId;
            var dbEmail = result.Email;

            // (3) If the user exists but new, set the External Id
            if (dbExternalId == null)
            {
                // Update external Id in this tenant database
                await _appRepo.Users__SetExternalIdByUserId(userId, _externalId);

                // Update external Id in the central Admin database too (To avoid an awkward situation
                // where a user exists on the tenant but not on the Admin db, if they change their email in between)
                await _adminRepo.DirectoryUsers__SetExternalIdByEmail(_externalEmail, _externalId);
            }

            // (4) Handle edge case
            else if (dbExternalId != _externalId)
            {
                // Note: there is the edge case of identity providers who allow email recycling. I.e. we can get the same email twice with 
                // two different external Ids. This issue is so unlikely to naturally occur and cause problems here that we are not going
                // to handle it for now. It can however happen artificually if the application is re-configured to a new identity provider,
                // or if someone messed with the identity database directly, but again out of scope for now.
                throw new InvalidOperationException($"The sign-in email '{dbEmail}' already exists but with a different external Id. TenantId: {TenantId}.");
            }

            // (5) If the user's email address has changed at the identity server, update it locally
            else if (dbEmail != _externalEmail)
            {
                await _appRepo.Users__SetEmailByUserId(userId, _externalEmail);
                await _adminRepo.DirectoryUsers__SetEmailByExternalId(_externalId, _externalEmail);

                _logger.LogWarning($"A user's email has been updated from '{dbEmail}' to '{_externalEmail}'. TenantId: {TenantId}.");
            }

            // (6) Mark this initializer as initialized and set the versions.
            _versions.SettingsVersion = result.SettingsVersion.ToString();
            _versions.DefinitionsVersion = result.DefinitionsVersion.ToString();
            _versions.UserSettingsVersion = result.UserSettingsVersion?.ToString();
            _versions.PermissionsVersion = result.PermissionsVersion?.ToString();

            _userEmail = UserEmail;
            _userId = userId;

            IsInitialized = true;

            // (7) Return the user Id 
            return userId;
        }

        public int TenantId => _tenantId;
        public ApplicationRepository Repository => _appRepo;
    }
}

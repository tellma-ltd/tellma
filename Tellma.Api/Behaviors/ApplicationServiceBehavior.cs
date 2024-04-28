using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
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
        private readonly IIdentityProxy _identityProxy;
        private readonly IStringLocalizer<Strings> _localizer;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        public ApplicationServiceBehavior(
            IApplicationRepositoryFactory repositoryFactory,
            ApplicationVersions versions,
            AdminRepository adminRepo,
            IIdentityProxy identityProxy,
            IStringLocalizer<Strings> localizer,
            IMemoryCache cache,
            ILogger<ApplicationServiceBehavior> logger)
        {
            _repositoryFactory = repositoryFactory;
            _versions = versions;
            _adminRepo = adminRepo;
            _identityProxy = identityProxy;
            _localizer = localizer;
            _cache = cache;
            _logger = logger;
        }

        private int _tenantId;
        private ApplicationRepository _appRepo;

        public bool IsInitialized { get; private set; } = false;

        public string SettingsVersion => IsInitialized ? _versions.SettingsVersion :
            throw new InvalidOperationException($"Accessing {nameof(SettingsVersion)} before initializing the service.");
        public string DefinitionsVersion => IsInitialized ? _versions.DefinitionsVersion :
            throw new InvalidOperationException($"Accessing {nameof(DefinitionsVersion)} before initializing the service.");
        public string UserSettingsVersion => IsInitialized ? _versions.UserSettingsVersion :
            throw new InvalidOperationException($"Accessing {nameof(UserSettingsVersion)} before initializing the service.");
        public string PermissionsVersion => IsInitialized ? _versions.PermissionsVersion :
            throw new InvalidOperationException($"Accessing {nameof(PermissionsVersion)} before initializing the service.");

        private bool _isAnonymous;
        private string _userEmail;
        private int _userId;
        private string _externalId;
        private bool _isServiceAccount;

        public bool IsAnonymous => IsInitialized ? _isAnonymous :
            throw new InvalidOperationException($"Accessing {nameof(IsAnonymous)} before initializing the service.");
        protected string UserEmail => IsInitialized ? _userEmail :
            throw new InvalidOperationException($"Accessing {nameof(UserEmail)} before initializing the service.");
        protected int UserId => IsInitialized ? _userId :
            throw new InvalidOperationException($"Accessing {nameof(UserId)} before initializing the service.");
        public int TenantId => IsInitialized ? _tenantId :
            throw new InvalidOperationException($"Accessing {nameof(TenantId)} before initializing the service.");
        public ApplicationRepository Repository => IsInitialized ? _appRepo :
            throw new InvalidOperationException($"Accessing {nameof(Repository)} before initializing the service.");

        public virtual async Task<int> OnInitialize(IServiceContextAccessor context, CancellationToken cancellation)
        {
            IsInitialized = true;
            bool isSilent = context.IsSilent;
            _isAnonymous = context.IsAnonymous;

            // Determine if this is a userless account
            if (_isAnonymous)
            {
                // (1) Call OnConnect...
                _tenantId = context.TenantId ?? throw new ServiceException($"Tenant id was not supplied.");
                _appRepo = _repositoryFactory.GetRepository(_tenantId);

                // Retrieve the settings version and the definitions version
                var result = await _appRepo.OnConnect(
                    externalUserId: null,
                    userEmail: null,
                    isServiceAccount: false,
                    setLastActive: !isSilent,
                    cancellation: cancellation);

                // (2) Set the versions and mark this initializer as initialized
                _versions.SettingsVersion = result.SettingsVersion.ToString();
                _versions.DefinitionsVersion = result.DefinitionsVersion.ToString();
                _versions.AreSet = true;

                return 0; // No user Id
            }
            else
            {
                // (1) Extract information from the Context Accessor
                bool isServiceAccount = context.IsServiceAccount;
                string externalId;
                string externalEmail = null;
                if (isServiceAccount)
                {
                    externalId = context.ExternalClientId ??
                        throw new InvalidOperationException($"The external client ID was not supplied.");
                }
                else
                {
                    // This is a human user, so the external Id and email are required
                    externalId = context.ExternalUserId ??
                        throw new InvalidOperationException($"The external user ID was not supplied.");
                    externalEmail = context.ExternalEmail ??
                        throw new InvalidOperationException($"The external user email was not supplied.");
                }

                _tenantId = context.TenantId ?? throw new ServiceException($"Tenant id was not supplied.");
                _appRepo = _repositoryFactory.GetRepository(_tenantId);


                // (2) Call OnConnect...
                // The client sometimes makes ambient (silent) API calls, not in response to
                // user interaction, such calls should not update LastAccess of that user
                var result = await _appRepo.OnConnect(
                    externalUserId: externalId,
                    userEmail: externalEmail,
                    isServiceAccount: isServiceAccount,
                    setLastActive: !isSilent,
                    cancellation: cancellation);

                // (3) Make sure the user is a member of this tenant
                if (result.UserId == null)
                {
                    // Either 1) the user is not a member in the database, or 2) the database does not exist
                    // Either way we return the not-member exception so as not to convey information to an attacker
                    throw new ForbiddenException(ForbiddenReason.NotCompanyMember, _localizer["Error_UnauthorizedForCompany"]);
                }

                // Extract values from the result
                var userId = result.UserId.Value;
                var dbExternalId = result.ExternalId;
                var dbEmail = result.Email;

                // (4) If the user exists but new, set the External Id
                if (dbExternalId == null)
                {
                    // Only possible with human users
                    // Update external Id in this tenant database
                    await _appRepo.Users__SetExternalIdByUserId(userId, externalId);

                    // Update external Id in the central Admin database too (To avoid an awkward situation
                    // where a user exists on the tenant but not on the Admin db, if they change their email in between)
                    await _adminRepo.DirectoryUsers__SetExternalIdByEmail(externalEmail, externalId);
                }

                // (5) Handle edge case
                else if (dbExternalId != externalId)
                {
                    // Only possible with human users

                    // Note: there is the edge case of identity providers who allow email recycling. I.e. we can get the same email twice with 
                    // two different external Ids. This issue is so unlikely to naturally occur and cause problems here that we are not going
                    // to handle it for now. It can however happen artificially if the application is re-configured to a new identity provider,
                    // or if someone messed with the identity database directly, but again out of scope for now.
                    throw new InvalidOperationException($"The sign-in email '{dbEmail}' already exists but with a different external Id. TenantId: {TenantId}.");
                }

                // (6) If the user's email address has changed at the identity server, update it locally
                else if (dbEmail != externalEmail && !isServiceAccount)
                {
                    await _appRepo.Users__SetEmailByUserId(userId, externalEmail);
                    await _adminRepo.DirectoryUsers__SetEmailByExternalId(externalId, externalEmail);

                    _logger.LogWarning($"A user's email has been updated from '{dbEmail}' to '{externalEmail}'. TenantId: {TenantId}.");
                }

                // (7) Set the versions and mark this initializer as initialized
                _versions.SettingsVersion = result.SettingsVersion.ToString();
                _versions.DefinitionsVersion = result.DefinitionsVersion.ToString();
                _versions.UserSettingsVersion = result.UserSettingsVersion?.ToString();
                _versions.PermissionsVersion = result.PermissionsVersion?.ToString();
                _versions.AreSet = true;

                _userEmail = dbEmail;
                _userId = userId;
                _externalId = externalId;
                _isServiceAccount = isServiceAccount;

                // (8) Ensure that the user account is consistent with company policies
                if (!isServiceAccount)
                {
                    // 2FA on local account policy
                    if (result.Enforce2faOnLocalAccounts && !await CachedUserHas2faEnabled())
                    {
                        throw new ForbiddenException(ForbiddenReason.ViolatesCompanyPolicy, _localizer["Error_MustEnable2FA"]);
                    }

                    // No external accounts policy
                    if (result.EnforceNoExternalAccounts && await CachedUserHasLinkedExternalAccounts())
                    {
                        throw new ForbiddenException(ForbiddenReason.ViolatesCompanyPolicy, _localizer["Error_MustUnlinkExternalAccounts"]);
                    }
                }

                // (9) Return the user Id 
                return userId;
            }
        }

        private async Task<bool> CachedUserHas2faEnabled()
        {
            const double CACHE_EXPIRY_IN_HOURS = 24;
            static string CacheKey(string exId)
            {
                return $"{nameof(_identityProxy.UserHas2faEnabled)}/{exId}";
            }

            if (!_cache.TryGetValue(CacheKey(_externalId), out bool result) || !result) // If cached value is FALSE, don't trust it, otherwise the user is locked out until the cache expires
            {
                // Get from the source
                result = await UserHas2faEnabled();
                var expiry = DateTimeOffset.UtcNow.AddHours(CACHE_EXPIRY_IN_HOURS);
                _cache.Set(CacheKey(_externalId), result, expiry);
            }

            return result;
        }

        private async Task<bool> CachedUserHasLinkedExternalAccounts()
        {
            const double CACHE_EXPIRY_IN_HOURS = 24;
            static string CacheKey(string exId)
            {
                return $"{nameof(_identityProxy.UserHasLinkedExternalAccounts)}/{exId}";
            }

            if (!_cache.TryGetValue(CacheKey(_externalId), out bool result) || result) // If cached value is TRUE, don't trust it, otherwise the user is locked out until the cache expires
            {
                // Get from the source
                result = await UserHasLinkedExternalAccounts();
                var expiry = DateTimeOffset.UtcNow.AddHours(CACHE_EXPIRY_IN_HOURS);
                _cache.Set(CacheKey(_externalId), result, expiry);
            }

            return result;
        }

        public async Task<bool> UserHas2faEnabled()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"Accessing {nameof(UserHasLinkedExternalAccounts)} before initializing the service.");
            }

            if (_isServiceAccount)
            {
                return true;
            }

            using var suppress = TransactionFactory.Suppress();
            return await _identityProxy.UserHas2faEnabled(_externalId);
        }

        public async Task<bool> UserHasLinkedExternalAccounts()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"Accessing {nameof(UserHasLinkedExternalAccounts)} before initializing the service.");
            }

            if (_isServiceAccount)
            {
                return false;
            }

            using var suppress = TransactionFactory.Suppress();
            return await _identityProxy.UserHasLinkedExternalAccounts(_externalId);
        }
    }
}

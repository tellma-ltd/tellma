using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Base;
using Tellma.Repository.Admin;

namespace Tellma.Api.Behaviors
{
    public class AdminServiceBehavior : IServiceBehavior
    {
        private readonly AdminRepository _adminRepo;
        private readonly ILogger _logger;

        private readonly string _externalId;
        private readonly string _externalEmail;
        private readonly CancellationToken _cancellation;


        public AdminServiceBehavior(
            IServiceContextAccessor context,
            AdminRepository adminRepo,
            ILogger<AdminServiceBehavior> logger)
        {
            _adminRepo = adminRepo;
            _logger = logger;

            _externalId = context.ExternalUserId ?? throw new ServiceException($"External user id was not supplied.");
            _externalEmail = context.ExternalEmail ?? throw new ServiceException($"External user email was not supplied.");
            _cancellation = context.Cancellation;

        }

        private bool _isInitialized = false;
        private string _userSettingsVersion;
        private string _permissionsVersion;

        public bool IsInitialized => _isInitialized;

        public string UserSettingsVersion => IsInitialized ? _userSettingsVersion :
            throw new InvalidOperationException($"Accessing {nameof(UserSettingsVersion)} before initializing the service.");
        public string PermissionsVersion => IsInitialized ? _permissionsVersion :
            throw new InvalidOperationException($"Accessing {nameof(PermissionsVersion)} before initializing the service.");
        public AdminRepository Repository => IsInitialized ? _adminRepo :
            throw new InvalidOperationException($"Accessing {nameof(Repository)} before initializing the service.");

        public async Task<int> OnInitialize()
        {
            // (1) Call OnConnect...
            var result = await _adminRepo.OnConnect(_externalId, _externalEmail, _cancellation);

            // (2) Make sure the user is a member of the admin database
            if (result.UserId == null)
            {
                throw new ForbiddenException(notMember: true);
            }


            var userId = result.UserId.Value;
            var dbExternalId = result.ExternalId;
            var dbEmail = result.Email;

            // (3) If the user exists but new, set the External Id
            if (dbExternalId == null)
            {
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                await _adminRepo.AdminUsers__SetExternalIdByUserId(userId, _externalId);
                await _adminRepo.DirectoryUsers__SetExternalIdByEmail(_externalEmail, _externalId);

                trx.Complete();
            }

            else if (dbExternalId != _externalId)
            {
                // Note: there is the edge case of identity providers who allow email recycling. I.e. we can get the same email twice with 
                // two different external Ids. This issue is so unlikely to naturally occur and cause problems here that we are not going
                // to handle it for now. It can however happen artificually if the application is re-configured to a new identity provider,
                // or if someone messed with the identity database directly, but again out of scope for now.
                throw new InvalidOperationException($"The sign-in email '{dbEmail}' already exists but with a different external Id. TenantId: Admin.");
            }

            // (4) If the user's email address has changed at the identity server, update it locally
            else if (dbEmail != _externalEmail)
            {
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                await _adminRepo.AdminUsers__SetEmailByUserId(userId, _externalEmail);
                await _adminRepo.DirectoryUsers__SetEmailByExternalId(_externalId, _externalEmail);

                _logger.LogWarning($"A user's email has been updated from '{dbEmail}' to '{_externalEmail}'. TenantId: Admin.");

                trx.Complete();
            }

            // (5) Mark this initializer as initialized and set the versions.
            _isInitialized = true;
            _userSettingsVersion = result.UserSettingsVersion?.ToString();
            _permissionsVersion = result.PermissionsVersion?.ToString();

            // (6) Return the user Id 
            return userId;
        }
    }
}

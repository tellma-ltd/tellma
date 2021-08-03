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
        private readonly AdminVersions _versions;
        private readonly ILogger _logger;

        private readonly string _externalId;
        private readonly string _externalEmail;

        public AdminServiceBehavior(
            IServiceContextAccessor context,
            AdminRepository adminRepo,
            AdminVersions versions,
            ILogger<AdminServiceBehavior> logger)
        {
            _versions = versions;
            _logger = logger;

            _externalId = context.ExternalUserId ?? throw new ServiceException($"External user id was not supplied.");
            _externalEmail = context.ExternalEmail ?? throw new ServiceException($"External user email was not supplied.");
            _adminRepo = adminRepo;
        }

        public bool IsInitialized { get; private set; } = false;

        public string UserSettingsVersion => IsInitialized ? _versions.UserSettingsVersion :
            throw new InvalidOperationException($"Accessing {nameof(UserSettingsVersion)} before initializing the service.");
        public string PermissionsVersion => IsInitialized ? _versions.PermissionsVersion :
            throw new InvalidOperationException($"Accessing {nameof(PermissionsVersion)} before initializing the service.");

        private string _userEmail;
        private int _userId;

        protected string UserEmail => IsInitialized ? _userEmail :
            throw new InvalidOperationException($"Accessing {nameof(UserEmail)} before initializing the service.");
        protected int UserId => IsInitialized ? _userId :
            throw new InvalidOperationException($"Accessing {nameof(UserId)} before initializing the service.");

        public AdminRepository Repository => IsInitialized ? _adminRepo :
            throw new InvalidOperationException($"Accessing {nameof(Repository)} before initializing the service.");

        public async Task<int> OnInitialize(CancellationToken cancellation)
        {
            // (1) Call OnConnect...
            var result = await _adminRepo.OnConnect(_externalId, _externalEmail, cancellation);

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
                using var trx = TransactionFactory.ReadCommitted();

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
                using var trx = TransactionFactory.ReadCommitted();

                await _adminRepo.AdminUsers__SetEmailByUserId(userId, _externalEmail);
                await _adminRepo.DirectoryUsers__SetEmailByExternalId(_externalId, _externalEmail);

                _logger.LogWarning($"A user's email has been updated from '{dbEmail}' to '{_externalEmail}'. TenantId: Admin.");

                trx.Complete();
            }

            // (5) Set the versions and mark this initializer as initialized
            _versions.UserSettingsVersion = result.UserSettingsVersion?.ToString();
            _versions.PermissionsVersion = result.PermissionsVersion?.ToString();
            _versions.AreSet = true;

            _userEmail = dbEmail;
            _userId = userId;

            IsInitialized = true;

            // (6) Return the user Id 
            return userId;
        }
    }
}

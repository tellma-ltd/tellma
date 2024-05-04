using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Repository.Admin;

namespace Tellma.Api.Behaviors
{
    public class AdminServiceBehavior : IServiceBehavior
    {
        private readonly AdminRepository _adminRepo;
        private readonly AdminVersions _versions;
        private readonly ILogger _logger;

        public AdminServiceBehavior(
            AdminRepository adminRepo,
            AdminVersions versions,
            ILogger<AdminServiceBehavior> logger)
        {
            _adminRepo = adminRepo;
            _versions = versions;
            _logger = logger;
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

        public async Task<int> OnInitialize(IServiceContextAccessor context, CancellationToken cancellation)
        {
            // (1) Extract context
            var isServiceAccount = context.IsServiceAccount;
            bool isSilent = context.IsSilent;
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

            // (2) Call OnConnect...
            var result = await _adminRepo.OnConnect(
                externalUserId: externalId,
                userEmail: externalEmail,
                isServiceAccount: isServiceAccount,
                setLastActive: !isSilent,
                cancellation: cancellation);

            // (3) Make sure the user is a member of the admin database
            if (result.UserId == null)
            {
                throw new ForbiddenException(ForbiddenReason.NotCompanyMember, "Your account is not an instance admin.");
            }

            var userId = result.UserId.Value;
            var dbExternalId = result.ExternalId;
            var dbEmail = result.Email;

            // (4) If the user exists but new, set the External Id
            if (dbExternalId == null)
            {
                using var trx = TransactionFactory.ReadCommitted();

                await _adminRepo.AdminUsers__SetExternalIdByUserId(userId, externalId);
                await _adminRepo.DirectoryUsers__SetExternalIdByEmail(externalEmail, externalId);

                trx.Complete();
            }

            else if (dbExternalId != externalId)
            {
                // Note: there is the edge case of identity providers who allow email recycling. I.e. we can get the same email twice with 
                // two different external Ids. This issue is so unlikely to naturally occur and cause problems here that we are not going
                // to handle it for now. It can however happen artificually if the application is re-configured to a new identity provider,
                // or if someone messed with the identity database directly, but again out of scope for now.
                throw new InvalidOperationException($"The sign-in email '{dbEmail}' already exists but with a different external Id. TenantId: Admin.");
            }

            // (5) If the user's email address has changed at the identity server, update it locally
            else if (dbEmail != externalEmail && !isServiceAccount)
            {
                using var trx = TransactionFactory.ReadCommitted();

                await _adminRepo.AdminUsers__SetEmailByUserId(userId, externalEmail);
                await _adminRepo.DirectoryUsers__SetEmailByExternalId(externalId, externalEmail);

                _logger.LogWarning($"An admin user's email has been updated from '{dbEmail}' to '{externalEmail}'.");

                trx.Complete();
            }

            // (6) Set the versions and mark this initializer as initialized
            _versions.UserSettingsVersion = result.UserSettingsVersion?.ToString();
            _versions.PermissionsVersion = result.PermissionsVersion?.ToString();
            _versions.AreSet = true;

            _userEmail = dbEmail;
            _userId = userId;

            IsInitialized = true;

            // (7) Return the user Id 
            return userId;
        }
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Repository.Admin;

namespace Tellma.Controllers
{
    public class AdminServiceInitializer : IServiceInitializer
    {
        private readonly AdminRepository _adminRepo;
        private readonly ILogger _logger;

        public AdminServiceInitializer(
            AdminRepository adminRepo,
            ILogger<AdminServiceInitializer> logger)
        {
            _adminRepo = adminRepo;
            _logger = logger;
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

        public async Task<int> OnInitialize(ServiceContext ctx)
        {
            // Extract the relevant context information
            var ctxExternalId = ctx.ExternalUserId;
            var ctxEmail = ctx.ExternalEmail;
            var cancellation = ctx.Cancellation;

            // (1) Call OnConnect...
            var result = await _adminRepo.OnConnect(ctxExternalId, ctxEmail, cancellation);

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

                await _adminRepo.AdminUsers__SetExternalIdByUserId(userId, ctxExternalId);
                await _adminRepo.DirectoryUsers__SetExternalIdByEmail(ctxEmail, ctxExternalId);

                trx.Complete();
            }

            else if (dbExternalId != ctxExternalId)
            {
                // Note: there is the edge case of identity providers who allow email recycling. I.e. we can get the same email twice with 
                // two different external Ids. This issue is so unlikely to naturally occur and cause problems here that we are not going
                // to handle it for now. It can however happen artificually if the application is re-configured to a new identity provider,
                // or if someone messed with the identity database directly, but again out of scope for now.
                throw new InvalidOperationException($"The sign-in email '{dbEmail}' already exists but with a different external Id. TenantId: Admin.");
            }

            // (4) If the user's email address has changed at the identity server, update it locally
            else if (dbEmail != ctxEmail)
            {
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                await _adminRepo.AdminUsers__SetEmailByUserId(userId, ctxEmail);
                await _adminRepo.DirectoryUsers__SetEmailByExternalId(ctxExternalId, ctxEmail);

                _logger.LogWarning($"A user's email has been updated from '{dbEmail}' to '{ctxEmail}'. TenantId: Admin.");

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

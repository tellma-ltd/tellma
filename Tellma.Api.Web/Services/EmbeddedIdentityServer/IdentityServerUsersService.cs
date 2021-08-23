using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Admin;
using Tellma.Repository.Common;

namespace Tellma.Services.EmbeddedIdentityServer
{
    public class IdentityServerUsersService : FactGetByIdServiceBase<IdentityServerUser, string>
    {
        private readonly AdminFactServiceBehavior _behavior;
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;
        private readonly IStringLocalizer _localizer;

        public IdentityServerUsersService(
            AdminFactServiceBehavior behavior,
            FactServiceDependencies deps,
            UserManager<EmbeddedIdentityServerUser> userManager) : base(deps)
        {
            _behavior = behavior;
            _userManager = userManager;
            _localizer = deps.Localizer;
        }

        protected override string View => "identity-server-users";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public async Task<EntitiesResult<IdentityServerUser>> ResetPassword(ResetPasswordArguments args)
        {
            await Initialize();

            // Check permissions
            List<string> idSingleton = new() { args.UserId }; // A single Id
            var actionFilter = await UserPermissionsFilter("ResetPassword", cancellation: default);
            idSingleton = await CheckActionPermissionsBefore(actionFilter, idSingleton);

            // Invisible or missing user
            if (!idSingleton.Any())
            {
                // The user cannot view that user, we pretend it doesn't exist
                throw new NotFoundException<string>(args.UserId);
            }

            // Some basic validation
            if (string.IsNullOrWhiteSpace(args.Password))
            {
                throw new ServiceException(_localizer["Error_Field0IsRequired", _localizer["Password"]]);
            }

            // Some basic validation
            if (string.IsNullOrWhiteSpace(args.UserId))
            {
                // Developer mistake
                throw new ServiceException(_localizer["Error_Field0IsRequired", nameof(args.UserId)]);
            }

            if (_userManager == null)
            {
                throw new InvalidOperationException($"Bug: Could not resolve UserManager in {nameof(IdentityServerUsersService)}.");
            }

            // Go ahead and reset the password as specified
            var user = await _userManager.FindByIdAsync(args.UserId);
            if (user == null)
            {
                throw new NotFoundException<string>(idSingleton);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, args.Password);

            // IF something goes wrong report an error
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                var errorMessage = string.Join(", ", errors);
                throw new ServiceException(errorMessage);
            }

            return await GetByIds(idSingleton, null, cancellation: default);
        }

        protected override Task<EntityQuery<IdentityServerUser>> Search(EntityQuery<IdentityServerUser> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var email = nameof(IdentityServerUser.Email);

                var filterString = $"{email} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
        }
    }
}

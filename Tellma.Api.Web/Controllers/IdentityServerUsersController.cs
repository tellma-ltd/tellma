using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Model.Application;
using Tellma.Services.EmbeddedIdentityServer;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [AdminController]
    public class IdentityServerUsersController : FactGetByIdControllerBase<IdentityServerUser, string>
    {
        public const string BASE_ADDRESS = "identity-server-users";

        private readonly IdentityServerUsersService _service;

        public IdentityServerUsersController(IdentityServerUsersService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("reset-password")]
        public async Task<ActionResult<EntitiesResponse<IdentityServerUser>>> ResetPassword(ResetPasswordArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.ResetPassword(args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            }
            , _logger);
        }

        protected override FactGetByIdServiceBase<IdentityServerUser, string> GetFactGetByIdService()
        {
            return _service;
        }
    }

    public class IdentityServerUsersService : FactGetByIdServiceBase<IdentityServerUser, string>
    {
        private readonly AdminRepository _adminRepo;
        private readonly IdentityRepository _idRepo;
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;

        private string View => IdentityServerUsersController.BASE_ADDRESS;

        public IdentityServerUsersService(
            AdminRepository adminRepo,
            IdentityRepository idRepo,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _adminRepo = adminRepo;
            _idRepo = idRepo;

            _userManager = (UserManager<EmbeddedIdentityServerUser>)serviceProvider.GetService(typeof(UserManager<EmbeddedIdentityServerUser>));

        }

        public async Task<(List<IdentityServerUser>, Extras)> ResetPassword(ResetPasswordArguments args)
        {
            // Check permissions
            var idSingleton = new List<string> { args.UserId }; // A single Id
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
                throw new BadRequestException(_localizer[Constants.Error_Field0IsRequired, _localizer["Password"]]);
            }

            // Some basic validation
            if (string.IsNullOrWhiteSpace(args.UserId))
            {
                // Developer mistake
                throw new BadRequestException(_localizer[Constants.Error_Field0IsRequired, nameof(args.UserId)]);
            }

            if (_userManager == null)
            {
                throw new InvalidOperationException($"Bug: Could not resolve UserManager in {nameof(IdentityServerUsersService)}");
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
                throw new BadRequestException(errorMessage);
            }

            return await GetByIds(idSingleton, null, Constants.Read, cancellation: default);
        }

        protected override IRepository GetRepository()
        {
            return _idRepo;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _adminRepo.UserPermissions(action, View, cancellation);
        }

        protected override Query<IdentityServerUser> Search(Query<IdentityServerUser> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var email = nameof(IdentityServerUser.Email);

                var filterString = $"{email} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return query;
        }
    }
}

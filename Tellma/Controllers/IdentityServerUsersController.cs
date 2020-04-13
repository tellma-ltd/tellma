using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.EmbeddedIdentityServer;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [AdminController]
    public class IdentityServerUsersController : FactGetByIdControllerBase<IdentityServerUser, string>
    {
        public const string BASE_ADDRESS = "identity-server-users";

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly AdminRepository _adminRepo;
        private readonly IdentityRepository _idRepo;
        private readonly UserManager<EmbeddedIdentityServerUser> _userManager;

        private string View => BASE_ADDRESS;

        public IdentityServerUsersController(
            ILogger<IdentityServerUsersController> logger,
            IStringLocalizer<Strings> localizer,
            AdminRepository adminRepo,
            IdentityRepository idRepo,
            UserManager<EmbeddedIdentityServerUser> userManager) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _adminRepo = adminRepo;
            _idRepo = idRepo;
            _userManager = userManager;
        }

        [HttpPut("reset-password")]
        public async Task<ActionResult<EntitiesResponse<IdentityServerUser>>> ResetPassword(ResetPasswordArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Check permissions
                var ids = new string[] { args.UserId }; // A single Id
                await CheckActionPermissions("ResetPassword", ids);

                // Some basic validation
                if (string.IsNullOrWhiteSpace(args.Password))
                {
                    throw new BadRequestException(_localizer[Services.Utilities.Constants.Error_TheField0IsRequired, _localizer["Password"]]);
                }

                // Some basic validation
                if (string.IsNullOrWhiteSpace(args.UserId))
                {
                    // Developer mistake
                    throw new BadRequestException(_localizer[Services.Utilities.Constants.Error_TheField0IsRequired, "UserId"]);
                }

                // Go ahead and reset the password as specified
                var user = await _userManager.FindByIdAsync(args.UserId);
                if (user == null)
                {
                    throw new NotFoundException<string>(ids);
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

                var response = await LoadDataByIdsAndTransform(ids, null, null);
                return Ok(response);
            }
            , _logger);
        }

        protected override IRepository GetRepository()
        {
            return _idRepo;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _adminRepo.UserPermissions(action, View, cancellation);
        }

        protected override Query<IdentityServerUser> Search(Query<IdentityServerUser> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var email = nameof(IdentityServerUser.Email);

                var filterString = $"{email} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }
    }
}

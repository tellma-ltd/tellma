using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.Identity;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using System.Linq;

namespace Tellma.Controllers
{
    /// <summary>
    /// 1. Ensures that the authenticated user has a valid admin user otherwise the request is aborted with a 403
    /// 2. If the user is new it updates his/her ExternalId in the admin database
    /// 3. If the user has a new email it updates his/her Email in the admin database
    /// 4. If version headers are provided, it also checks their freshness and adds appropriate response headers
    /// IMPORTANT: This attribute should always be precedede with another attribute <see cref="AuthorizeJwtBearerAttribute"/>
    /// </summary>
    public class AdminControllerAttribute : TypeFilterAttribute
    {
        public AdminControllerAttribute() : base(typeof(AdminApiFilter)) { }

        /// <summary>
        /// An implementation of the method described here https://bit.ly/2MKwY7A
        /// </summary>
        private class AdminApiFilter : IAsyncResourceFilter
        {
            private readonly AdminRepository _adminRepo;
            private readonly IExternalUserAccessor _externalUserAccessor;

            public AdminApiFilter(AdminRepository adminRepo, IExternalUserAccessor externalUserAccessor)
            {
                _adminRepo = adminRepo;
                _externalUserAccessor = externalUserAccessor;
            }

            public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
            {
                var cancellation = context.HttpContext.RequestAborted;

                // (1) Make sure the requester has an active user 
                AdminUserInfo userInfo = await _adminRepo.GetAdminUserInfoAsync(cancellation);

                if (userInfo.UserId == null)
                {
                    // If there is no user cut the pipeline short and return a Forbidden 403
                    context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);

                    // This indicates to the client to discard all cached information about this
                    // company since the user is no longer a member of it
                    context.HttpContext.Response.Headers.Add("x-admin-settings-version", Constants.Unauthorized);
                    context.HttpContext.Response.Headers.Add("x-admin-permissions-version", Constants.Unauthorized);
                    context.HttpContext.Response.Headers.Add("x-admin-user-settings-version", Constants.Unauthorized);

                    return;
                }

                var userId = userInfo.UserId.Value;
                var externalId = _externalUserAccessor.GetUserId();
                var externalEmail = _externalUserAccessor.GetUserEmail();

                // (3) If the user exists but new, set the External Id
                if (userInfo.ExternalId == null)
                {
                    using var trx = ControllerUtilities.CreateTransaction();

                    await _adminRepo.AdminUsers__SetExternalIdByUserId(userId, externalId);
                    await _adminRepo.DirectoryUsers__SetExternalIdByEmail(externalEmail, externalId);

                    trx.Complete();
                }

                else if (userInfo.ExternalId != externalId)
                {
                    // Note: we will assume that no identity provider can provider the same email twice with 
                    // two different external Ids, i.e. that no provider allows email recycling, so we won't handle this case now
                    // This can only happen if the application is re-configured to a new identity provider, or if someone messed with
                    // the database directly
                    context.Result = new BadRequestObjectResult("The sign-in email already exists but with a different external Id");
                    return;
                }

                // (4) If the user's email address has changed at the identity server, update it locally
                else if (userInfo.Email != externalEmail)
                {
                    using var trx = ControllerUtilities.CreateTransaction();
                    await _adminRepo.AdminUsers__SetEmailByUserId(userId, externalEmail);
                    await _adminRepo.DirectoryUsers__SetEmailByExternalId(externalId, externalEmail);

                    trx.Complete();
                }

                // (5) If any version headers are supplied: examine their freshness
                {
                    // Permissions
                    var clientVersion = context.HttpContext.Request.Headers["X-Admin-Permissions-Version"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        var databaseVersion = userInfo.PermissionsVersion;
                        context.HttpContext.Response.Headers.Add("x-admin-permissions-version",
                            clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                    }
                }

                {
                    // User Settings
                    var clientVersion = context.HttpContext.Request.Headers["X-Admin-User-Settings-Version"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        var databaseVersion = userInfo.UserSettingsVersion;
                        context.HttpContext.Response.Headers.Add("x-admin-user-settings-version",
                            clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                    }
                }

                {
                    // Settings
                    var clientVersion = context.HttpContext.Request.Headers["X-Admin-Settings-Version"].FirstOrDefault();
                    var adminInfo = new { SettingsVersion = clientVersion }; // await _adminRepo.GetAdminInfoAsync(); // TODO
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        var databaseVersion = adminInfo.SettingsVersion;
                        context.HttpContext.Response.Headers.Add("x-settings-version",
                            clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                    }
                }

                // Finally call the Action itself
                await next();
            }
        }
    }
}

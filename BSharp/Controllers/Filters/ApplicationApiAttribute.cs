using BSharp.Data;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.Identity;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    /// <summary>
    /// 1. Ensures that the API caller has supplied a valid tenantId value, otherwise the request is aborted with a 400
    /// 2. Ensures that the authenticated user has an active membership in that tenantId otherwise the request is aborted with a 403
    /// 3. If the user is new it updates his/her ExternalId in the tenant database as well as the centralized admin database
    /// 4. If the user has a new email it updates his/her Email in the app database
    /// 5. If the version headers are provided, it also checks their freshness and adds appropriate response headers
    /// IMPORTANT: This attribute should always be precedede with another attribute <see cref="AuthorizeAccessAttribute"/>
    /// </summary>
    public class ApplicationApiAttribute : TypeFilterAttribute
    {
        public ApplicationApiAttribute() : base(typeof(ApplicationApiFilter)) { }

        /// <summary>
        /// An implementation of the method described here https://bit.ly/2MKwY7A
        /// </summary>
        private class ApplicationApiFilter : IAsyncResourceFilter
        {
            private readonly ApplicationRepository _appRepo;
            private readonly ITenantIdAccessor _tenantIdAccessor;
            private readonly IExternalUserAccessor _externalUserAccessor;
            private readonly IServiceProvider _serviceProvider;

            public ApplicationApiFilter(ITenantIdAccessor tenantIdAccessor, ApplicationRepository appRepo, 
                IExternalUserAccessor externalUserAccessor, IServiceProvider serviceProvider)
            {
                _appRepo = appRepo;
                _tenantIdAccessor = tenantIdAccessor;
                _externalUserAccessor = externalUserAccessor;
                _serviceProvider = serviceProvider;
            }

            public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
            {
                // (1) Make sure the API caller have provided a tenantId, and extract it
                int tenantId;
                try
                {
                    tenantId = _tenantIdAccessor.GetTenantId();
                }
                catch (MultitenancyException ex)
                {
                    // If the tenant Id is not provided cut the pipeline short and return a Bad Request 400
                    context.Result = new BadRequestObjectResult(ex.Message);
                    return;
                }

                // (2) Make sure the user is a member of this tenant
                UserInfo userInfo = await _appRepo.GetUserInfoAsync();
                
                if (userInfo.UserId == null)
                {
                    // If there is no user cut the pipeline short and return a Forbidden 403
                    context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);

                    // This indicates to the client to discard all cached information about this
                    // company since the user is no longer a member of it
                    context.HttpContext.Response.Headers.Add("x-settings-version", Constants.Unauthorized);
                    context.HttpContext.Response.Headers.Add("x-permissions-version", Constants.Unauthorized);
                    context.HttpContext.Response.Headers.Add("x-user-settings-version", Constants.Unauthorized);

                    return;
                }

                var userId = userInfo.UserId.Value;
                var externalId = _externalUserAccessor.GetUserId();
                var externalEmail = _externalUserAccessor.GetUserEmail();

                // (3) If the user exists but new, set the External Id
                if (userInfo.ExternalId == null)
                {
                    // Update external Id in this tenant database
                    await _appRepo.Users__SetExternalIdByUserId(userId, externalId);

                    // Update external Id in the central Admin database too (To avoid an awkward situation
                    // where a user exists on the tenant but not on the Admin db, if they change their email in between)
                    var adminApp = _serviceProvider.GetRequiredService<AdminRepository>();
                    await adminApp.GlobalUsers__SetExternalIdByEmail(externalEmail, externalId);
                }

                else if(userInfo.ExternalId != externalId)
                {
                    // Note: there is the edge case of identity providers who allow email recycling. I.e. we can get the same email twice with 
                    // two different external Ids. This issue is so unlikely to naturally occur and cause problems here that we are not going
                    // to handle it for now. It can however happen artificually if the application is re-configured to a new identity provider,
                    // or if someone messed with database directly, but again out of scope for now.
                    context.Result = new BadRequestObjectResult("The sign-in email already exists but with a different external Id");
                    return;
                }

                // (4) If the user's email address has changed at the identity server, update it locally
                else if (userInfo.Email != externalEmail)
                {
                    await _appRepo.Users__SetEmailByUserId(userId, externalEmail);
                }


                // (5) If any version headers are supplied: examine their freshness
                {
                    // Permissions
                    var clientVersion = context.HttpContext.Request.Headers["X-Permissions-Version"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        var serverVersion = userInfo.PermissionsVersion;
                        context.HttpContext.Response.Headers.Add("x-permissions-version",
                            clientVersion == serverVersion ? Constants.Fresh : Constants.Stale);
                    }
                }

                {
                    // User Settings
                    var clientVersion = context.HttpContext.Request.Headers["X-User-Settings-Version"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        var serverVersion = userInfo.UserSettingsVersion;
                        context.HttpContext.Response.Headers.Add("x-user-settings-version",
                            clientVersion == serverVersion ? Constants.Fresh : Constants.Stale);
                    }
                }

                var tenantInfo = await _appRepo.GetTenantInfoAsync();
                {
                    // Settings
                    var clientVersion = context.HttpContext.Request.Headers["X-Settings-Version"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        var serverVersion = tenantInfo.SettingsVersion;
                        context.HttpContext.Response.Headers.Add("x-settings-version",
                            clientVersion == serverVersion ? Constants.Fresh : Constants.Stale);
                    }
                }

                // Call the Action itself
                await next();
            }
        }
    }
}

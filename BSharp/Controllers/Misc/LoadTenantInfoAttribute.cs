using BSharp.Data;
using BSharp.Services.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using BSharp.Services.Utilities;
using BSharp.Services.ApiAuthentication;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// Loads general contextual information about the user and the tenant and stores them in the HTTP Context,
    /// this information is accessible with the <see cref="ITenantUserInfoAccessor"/> service. 
    /// IMPORTANT: This attribute should always be precedede with another attribute <see cref="AuthorizeAccessAttribute"/>
    /// </summary>
    public class LoadTenantInfoAttribute : TypeFilterAttribute
    {
        public LoadTenantInfoAttribute() : base(typeof(LoadTenantInfo))
        {
        }

        private class LoadTenantInfo : IResourceFilter
        {
            private readonly IServiceProvider _provider;

            public LoadTenantInfo(IServiceProvider provider)
            {
                _provider = provider;
            }

            public void OnResourceExecuting(ResourceExecutingContext context)
            {
                // (1) Make sure the API caller have provided a tenantId
                var tenantIdProvider = _provider.GetRequiredService<ITenantIdAccessor>();
                if (!tenantIdProvider.HasTenantId())
                {
                    // If there is no tenant Id header cut the pipeline short and return a Forbidden result
                    context.Result = new BadRequestObjectResult($"The required header '{TenantIdAccessor.REQUEST_HEADER_TENANT_ID}' was not supplied");
                    return;
                }


                // (2) Make sure the user is a member of this tenant
                // Note: To initialize ITenantUserInfo, we simply resolve ApplicationContext from the DI
                // container the constructor of ApplicationContext automatically does the deed
                var appContext = (ApplicationContext)_provider.GetService(typeof(ApplicationContext));

                // Retrieve the TenantUserInfo with help from the DI container
                var tenantInfoAccessor = _provider.GetRequiredService<ITenantUserInfoAccessor>();
                var tenantInfo = tenantInfoAccessor.GetCurrentInfo();

                if (tenantInfo.UserId == null && tenantInfo.Email == null && tenantInfo.ExternalId == null)
                {
                    // If there is no user cut the pipeline short and return a Forbidden result
                    context.Result = new StatusCodeResult(403);

                    // This indicates to the client to discard all cached information about this
                    // company since the user is no longer a member of it
                    context.HttpContext.Response.Headers.Add("x-settings-version", Constants.Unauthorized);
                    context.HttpContext.Response.Headers.Add("x-permissions-version", Constants.Unauthorized);
                    context.HttpContext.Response.Headers.Add("x-user-settings-version", Constants.Unauthorized);
                    return;
                }


                // (3) If the user exists but new, set the External Id
                var userId = tenantInfo.UserId.Value;
                var externalId = context.HttpContext.User.ExternalUserId();
                var email = context.HttpContext.User.Email();

                if (tenantInfo.ExternalId != externalId)
                {
                    // Update external Id in this tenant and in all other tenants where this user is registered
                    appContext.Database.ExecuteSqlCommandAsync($"UPDATE [dbo].[LocalUsers] SET ExternalId = {externalId} WHERE Email = {email}");

                    var adminContext = _provider.GetRequiredService<AdminContext>();
                    adminContext.Database.ExecuteSqlCommandAsync($"UPDATE [dbo].[GlobalUsers] SET ExternalId = {externalId} WHERE Email = {email}");
                }


                // (4) If the user's email address has changed, do the needful
                // TODO

                // (5) If the user's email exists but his id changed, do the needful


                // (5) If any version headers are supplied: confirm their freshness
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

                {
                    // Permissions
                    var clientVersion = context.HttpContext.Request.Headers["X-Permissions-Version"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        var serverVersion = tenantInfo.PermissionsVersion;
                        context.HttpContext.Response.Headers.Add("x-permissions-version",
                            clientVersion == serverVersion ? Constants.Fresh : Constants.Stale);
                    }
                }

                {
                    // User Settings
                    var clientVersion = context.HttpContext.Request.Headers["X-User-Settings-Version"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        var serverVersion = tenantInfo.UserSettingsVersion;
                        context.HttpContext.Response.Headers.Add("x-user-settings-version",
                            clientVersion == serverVersion ? Constants.Fresh : Constants.Stale);
                    }
                }
            }

            public void OnResourceExecuted(ResourceExecutedContext context)
            {
            }
        }
    }
}

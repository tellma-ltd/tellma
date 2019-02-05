using BSharp.Data;
using BSharp.Services.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace BSharp.Controllers.Misc
{
    public class LoadTenantInfoAttribute : TypeFilterAttribute
    {
        public LoadTenantInfoAttribute() : base(typeof(LoadTenantInfo))
        {
        }

        private class LoadTenantInfo : IResourceFilter
        {
            private const string FRESH = "Fresh";
            private const string STALE = "Stale";
            private const string UNAUTHORIZED = "Unauthorized";


            private readonly IServiceProvider _provider;

            public LoadTenantInfo(IServiceProvider provider)
            {
                _provider = provider;
            }

            public void OnResourceExecuting(ResourceExecutingContext context)
            {
                // (1) Make sure the API caller have provided a tenantId
                var tenantIdProvider = (ITenantIdProvider)_provider.GetService(typeof(ITenantIdProvider));
                if (!tenantIdProvider.HasTenantId())
                {
                    // If there is no tenant Id header cut the pipeline short and return a Forbidden result
                    context.Result = new BadRequestObjectResult($"The required header '{TenantIdProvider.REQUEST_HEADER_TENANT_ID}' was not supplied");
                    return;
                }


                // (2) Make sure the user is a member of this tenant
                // Note: To initialize ITenantUserInfo, we simply resolve ApplicationContext from the DI
                // container the constructor of ApplicationContext automatically does the deed
                _provider.GetService(typeof(ApplicationContext));

                // Retrieve the TenantUserInfo with help from the DI container
                var tenantInfoAccessor = (ITenantUserInfoAccessor)_provider.GetService(typeof(ITenantUserInfoAccessor));
                var tenantInfo = tenantInfoAccessor.GetCurrentInfo();

                if (tenantInfo.UserId == null && tenantInfo.Email == null && tenantInfo.ExternalId == null)
                {
                    // If there is no user cut the pipeline short and return a Forbidden result
                    context.Result = new StatusCodeResult(403);

                    // This indicates to the client to discard all cached information about this
                    // company since the user is no longer a member of it
                    context.HttpContext.Response.Headers.Add("x-settings-version", UNAUTHORIZED);
                    context.HttpContext.Response.Headers.Add("x-permissions-version", UNAUTHORIZED);
                    context.HttpContext.Response.Headers.Add("x-user-settings-version", UNAUTHORIZED);
                    return;
                }


                // (3) If the user exists but new, do the needful
                // TODO


                // (4) If the user's email address has changed, do the needful
                // TODO


                // (5) If any version headers are supplied: confirm their freshness
                {
                    // Settings
                    var clientVersion = context.HttpContext.Request.Headers["X-Settings-Version"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        var serverVersion = tenantInfo.SettingsVersion;
                        context.HttpContext.Response.Headers.Add("x-settings-version",
                            clientVersion == serverVersion ? FRESH : STALE);
                    }
                }

                {
                    // Permissions
                    var clientVersion = context.HttpContext.Request.Headers["X-Permissions-Version"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        var serverVersion = tenantInfo.PermissionsVersion;
                        context.HttpContext.Response.Headers.Add("x-permissions-version",
                            clientVersion == serverVersion ? FRESH : STALE);
                    }
                }

                {
                    // User Settings
                    var clientVersion = context.HttpContext.Request.Headers["X-User-Settings-Version"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        var serverVersion = tenantInfo.UserSettingsVersion;
                        context.HttpContext.Response.Headers.Add("x-user-settings-version",
                            clientVersion == serverVersion ? FRESH : STALE);
                    }
                }
            }

            public void OnResourceExecuted(ResourceExecutedContext context)
            {
            }
        }
    }
}

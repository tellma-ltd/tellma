using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using Tellma.Api.Behaviors;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    public class AdminControllerAttribute : TypeFilterAttribute
    {
        public AdminControllerAttribute() : base(typeof(AdminFilter))
        {
        }

        private class AdminFilter : IActionFilter
        {

            private readonly AdminVersions _versions;

            public AdminFilter(AdminVersions versions)
            {
                _versions = versions;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                // If any version headers are supplied: examine their freshness
                if (_versions.AreSet)
                {
                    {
                        // Permissions
                        var clientVersion = context.HttpContext.Request.Headers["X-Admin-Permissions-Version"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(clientVersion))
                        {
                            var databaseVersion = _versions.PermissionsVersion;
                            context.HttpContext.Response.Headers.TrySet("x-admin-permissions-version",
                                clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                        }
                    }

                    {
                        // User Settings
                        var clientVersion = context.HttpContext.Request.Headers["X-Admin-User-Settings-Version"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(clientVersion))
                        {
                            var databaseVersion = _versions.UserSettingsVersion;
                            context.HttpContext.Response.Headers.TrySet("x-admin-user-settings-version",
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
                            context.HttpContext.Response.Headers.TrySet("x-settings-version",
                                clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                        }
                    }
                }
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
            }
        }
    }
}

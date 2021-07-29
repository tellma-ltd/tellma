using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using Tellma.Api.Behaviors;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    public class ApplicationControllerAttribute : TypeFilterAttribute
    {
        public ApplicationControllerAttribute() : base(typeof(ApplicationFilter))
        {
        }

        private class ApplicationFilter : IActionFilter
        {
            private readonly ApplicationVersions _versions;

            public ApplicationFilter(ApplicationVersions versions)
            {
                _versions = versions;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                // If any version headers are supplied: examine their freshness
                if (_versions.AreSet)
                {
                    var requestHeaders = context.HttpContext.Request.Headers;
                    var responseHeaders = context.HttpContext.Response.Headers;

                    {
                        // Permissions
                        var clientVersion = requestHeaders["X-Permissions-Version"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(clientVersion))
                        {
                            var databaseVersion = _versions.PermissionsVersion;
                            responseHeaders.TrySet("x-permissions-version",
                                clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                        }
                    }

                    {
                        // User Settings
                        var clientVersion = requestHeaders["X-User-Settings-Version"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(clientVersion))
                        {
                            var databaseVersion = _versions.UserSettingsVersion;
                            responseHeaders.TrySet("x-user-settings-version",
                                clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                        }
                    }

                    {
                        // Definitions
                        var clientVersion = requestHeaders["X-Definitions-Version"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(clientVersion))
                        {
                            var databaseVersion = _versions.DefinitionsVersion;
                            responseHeaders.TrySet("x-definitions-version",
                                clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                        }
                    }

                    {
                        // Settings
                        var clientVersion = requestHeaders["X-Settings-Version"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(clientVersion))
                        {
                            var databaseVersion = _versions.SettingsVersion;
                            responseHeaders.TrySet("x-settings-version",
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

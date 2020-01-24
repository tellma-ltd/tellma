using Tellma.Services.GlobalSettings;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Tellma.Controllers
{
    /// <summary>
    /// This filter is invoked with every API request, independent of any tenant.
    /// For now it checks version headers of any global settings.
    /// </summary>
    public class GlobalFilter : IResourceFilter
    {
        private readonly IGlobalSettingsCache _globalSettings;

        public GlobalFilter(IGlobalSettingsCache globalSettings)
        {
            _globalSettings = globalSettings;
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            // Global Settings
            {
                var clientVersion = context.HttpContext.Request.Headers["X-Global-Settings-Version"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(clientVersion))
                {
                    context.HttpContext.Response.Headers.Add("x-global-settings-version",
                         _globalSettings.IsFresh(clientVersion) ? Constants.Fresh : Constants.Stale);
                }
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }

}

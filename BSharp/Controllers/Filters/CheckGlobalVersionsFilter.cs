using BSharp.Services.GlobalSettings;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace BSharp.Controllers
{
    /// <summary>
    /// This filter checks version headers of global settings such as translations, it is not dependent on any tenant
    /// </summary>
    public class CheckGlobalVersionsFilter : IResourceFilter
    {
        private readonly IGlobalSettingsCache _globalSettings;

        public CheckGlobalVersionsFilter(IGlobalSettingsCache globalSettings)
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

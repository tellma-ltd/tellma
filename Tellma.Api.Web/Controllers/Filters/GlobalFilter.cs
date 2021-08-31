using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using Tellma.Api.Dto;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    /// <summary>
    /// This filter is invoked with every API request, independent of any tenant.
    /// For now it checks version headers of any global settings.
    /// </summary>
    public class GlobalFilter : IResourceFilter
    {
        private readonly GlobalSettingsProvider _globalSettingsProvider;

        public GlobalFilter(GlobalSettingsProvider globalSettingsProvider)
        {
            _globalSettingsProvider = globalSettingsProvider;
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            // Checks the client version of the Global Settings
            var clientVersion = context.HttpContext.Request.Headers["X-Global-Settings-Version"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(clientVersion))
            {
                bool isFresh = _globalSettingsProvider.IsFresh(clientVersion);
                context.HttpContext.Response.Headers.Add("x-global-settings-version", isFresh ? Constants.Fresh : Constants.Stale);
            }

            // Adds the server time
            var nowString = DateTimeOffsetConverter.ToString(DateTimeOffset.Now);
            context.HttpContext.Response.Headers.Add("x-server-time", nowString);
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }
}

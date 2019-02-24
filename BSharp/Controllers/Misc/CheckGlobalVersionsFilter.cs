using BSharp.Data;
using BSharp.Services.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using BSharp.Services.Utilities;
using BSharp.Services.ApiAuthentication;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using BSharp.Services.SqlLocalization;
using BSharp.Services.GlobalSettings;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// This filter checks version headers of global settings such as translations, it is not dependent on any tenant
    /// </summary>
    public class CheckGlobalVersionsFilter : IResourceFilter
    {
        private readonly ISqlStringLocalizerFactory _factory;
        private readonly IGlobalSettingsCache _globalSettings;

        public CheckGlobalVersionsFilter(ISqlStringLocalizerFactory factory, IGlobalSettingsCache globalSettings)
        {
            _factory = factory;
            _globalSettings = globalSettings;
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            // Confirm their freshness of any global version headers supplied
            var cultureName = CultureInfo.CurrentUICulture.Name;
            // Translations
            {
                var clientVersion = context.HttpContext.Request.Headers["X-Translations-Version"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(clientVersion))
                {
                    context.HttpContext.Response.Headers.Add("x-translations-version",
                         _factory.IsFresh(cultureName, clientVersion) ? Constants.Fresh : Constants.Stale);
                }
            }

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

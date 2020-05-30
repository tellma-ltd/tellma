using Tellma.Controllers.Dto;
using Tellma.Services.MultiTenancy;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Tellma.Data;

namespace Tellma.Controllers
{
    public class SettingsCache : ISettingsCache
    {
        private static string HttpContextKey(int databaseId) => $"REQUEST_SETTINGS/{databaseId}";

        /// <summary>
        /// Mapping from tenant Id to its <see cref="SettingsForClient"/>
        /// </summary>
        private static readonly ConcurrentDictionary<int, Versioned<SettingsForClient>> _cache
            = new ConcurrentDictionary<int, Versioned<SettingsForClient>>();

        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IHttpContextAccessor _contextAccessor;

        public SettingsCache(ITenantIdAccessor tenantIdAccessor, IHttpContextAccessor contextAccessor)
        {
            _tenantIdAccessor = tenantIdAccessor;
            _contextAccessor = contextAccessor;
        }

        /// <summary>
        /// Implementation of <see cref="ISettingsCache"/>
        /// </summary>
        public Versioned<SettingsForClient> GetSettingsIfCached(int tenantId)
        {
            // This first step ensures that the same settings are always returned within
            // the scope of a single request, even if another thread updates the cache
            var ctx = _contextAccessor.HttpContext;
            if (ctx.Items.TryGetValue(HttpContextKey(tenantId), out object defsObj) && defsObj is Versioned<SettingsForClient> settings)
            {
                return settings;
            }

            _cache.TryGetValue(tenantId, out settings);
            if (settings != null)
            {
                ctx.Items.Add(HttpContextKey(tenantId), settings);
            }

            return settings;
        }

        public Versioned<SettingsForClient> GetCurrentSettingsIfCached()
        {
            int tenantId = _tenantIdAccessor.GetTenantId();
            return GetSettingsIfCached(tenantId);
        }

        /// <summary>
        /// Implementation of <see cref="ISettingsCache"/>
        /// </summary>
        public void SetSettings(int tenantId, Versioned<SettingsForClient> Settings)
        {
            if (tenantId == 0)
            {
                throw new ArgumentException($"{nameof(tenantId)} must be provided");
            }

            if (Settings is null)
            {
                throw new ArgumentNullException(nameof(Settings));
            }

            _cache.AddOrUpdate(tenantId, Settings, (i, d) => Settings);
        }
    }

    public static class SettingsForClientExtensions
    {
        public static string Localize(this TenantInfo tenantInfo, string s, string s2, string s3)
        {
            var cultureName = System.Globalization.CultureInfo.CurrentUICulture.Name;

            var currentLangIndex = cultureName == tenantInfo.TernaryLanguageId ? 3 : cultureName == tenantInfo.SecondaryLanguageId ? 2 : 1;
            return currentLangIndex == 3 ? (s3 ?? s) : currentLangIndex == 2 ? (s2 ?? s) : s;
        }
        public static string Localize(this SettingsForClient settings, string s, string s2, string s3)
        {
            var cultureName = System.Globalization.CultureInfo.CurrentUICulture.Name;

            var currentLangIndex = cultureName == settings.TernaryLanguageId ? 3 : cultureName == settings.SecondaryLanguageId ? 2 : 1;
            return currentLangIndex == 3 ? (s3 ?? s) : currentLangIndex == 2 ? (s2 ?? s) : s;
        }
    }
}

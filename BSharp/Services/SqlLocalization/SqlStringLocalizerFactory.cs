using BSharp.Data;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BSharp.Services.SqlLocalization
{
    /// <summary>
    /// Relies on the translations stored in a SQL server database, this implementation uses a combination
    /// of distributed caching and local memory caching to achieve a high degree of performance, while still
    /// providing up-to-date localization
    /// </summary>
    public class SqlStringLocalizerFactory : ICachingStringLocalizerFactory
    {
        private const string CORE = "core";
        private const string HTTP_CONTEXT_FLAG_NAME = "IsCacheUpdated";
        private const int DISTRIBUTED_CACHE_EXPIRATION_DAYS = 30;

        /// <summary>
        /// Local Cache
        /// </summary>
        private static ConcurrentDictionary<string, LocalCacheItem> _localCache
            = new ConcurrentDictionary<string, LocalCacheItem>();

        private readonly IConfiguration _config;
        private readonly IDistributedCache _distributedCache;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SqlStringLocalizerFactory(
            IConfiguration config,
            IDistributedCache distributedCache,
            IServiceProvider serviceProvider,
            ITenantIdProvider tenantIdProvider,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _config = config.GetSection("Localization");
            _distributedCache = distributedCache;
            _serviceProvider = serviceProvider;
            _tenantIdProvider = tenantIdProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return CreateSqlStringLocalizer();
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return CreateSqlStringLocalizer();
        }

        private IStringLocalizer CreateSqlStringLocalizer()
        {
            string defaultUICulture = _config["DefaultUICulture"];
            string specificUICulture = CultureInfo.CurrentUICulture.Name;
            string neutralUICulture = CultureInfo.CurrentUICulture.IsNeutralCulture ? specificUICulture : CultureInfo.CurrentUICulture.Parent.Name;

            // We refresh the cache once per request, and we track this using a flag in the HTTP Context object
            var isUpdated = _httpContextAccessor.HttpContext == null ? false : _httpContextAccessor.HttpContext.Items[HTTP_CONTEXT_FLAG_NAME];
            if (isUpdated == null || !(bool)isUpdated)
            {
                // The list of cultures to update;
                List<string> culturesToFreshenUp = new List<string>
                {
                    // Update localization cache for the default application culture
                    defaultUICulture
                };

                // Update localization cache for the current request culture, if different
                if (specificUICulture != defaultUICulture)
                {
                    culturesToFreshenUp.Add(specificUICulture);
                }

                // Update localization cache for the neutral request culture, if not already neutral
                if (neutralUICulture != specificUICulture)
                {
                    culturesToFreshenUp.Add(neutralUICulture);
                }

                // Update the local cache, or distributed cache timestamp if need be
                EnsureFreshnessOfCaches(culturesToFreshenUp.ToArray());

                // Set the flag, to prevent another cache refresh within the same scope
                // Note: this is thread safe, since only one thread at a time will get
                // a copy of the HTTP context, since it is scoped per request
                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Items[HTTP_CONTEXT_FLAG_NAME] = true;
                }
            }

            var coreSpecificTranslations = _localCache[CacheKey(specificUICulture, CORE)]?.Translations;
            var coreNeutralUICulture = _localCache[CacheKey(neutralUICulture, CORE)]?.Translations;
            var coreDefaultTranslations = _localCache[CacheKey(defaultUICulture, CORE)]?.Translations;
            if (_tenantIdProvider.HasTenantId())
            {
                // If the request scope contains a tenant Id, return also the translations of that tenant Id
                string tenantId = _tenantIdProvider.GetTenantId().ToString();

                var tenantSpecificTranslations = _localCache[CacheKey(specificUICulture, tenantId)]?.Translations;
                var tenantNeutralUICulture = _localCache[CacheKey(neutralUICulture, tenantId)]?.Translations;
                var tenantDefaultTranslations = _localCache[CacheKey(defaultUICulture, tenantId)]?.Translations;

                return new SqlStringLocalizer(
                    coreSpecificTranslations,
                    coreNeutralUICulture,
                    coreDefaultTranslations,
                    tenantSpecificTranslations,
                    tenantNeutralUICulture,
                    tenantDefaultTranslations);
            }

            // Return the core translations
            return new SqlStringLocalizer(
                coreSpecificTranslations,
                coreNeutralUICulture,
                coreDefaultTranslations);
        }

        /// <summary>
        /// Checks the local cache timestamp against the distributed cache timestamp if the local timestamp
        /// is blank or old than the distributed cache's, it updates it with a fresh copy from the database, 
        /// it also updates it if the timestamp in the distributed cache is blank or invalid, this is
        /// performed for both core and tenant localizations if tenant id is available
        /// </summary>
        /// <param name="cultureName"></param>
        private void EnsureFreshnessOfCaches(params string[] cultureNames)
        {
            // Here we prepare a list of all the caches that will be checked 
            // based on the supplied culture names
            List<CacheInfo> cachesToCheck = new List<CacheInfo>();
            foreach (var cultureName in cultureNames)
            {
                // The cache representing the core translations in that specific culture
                cachesToCheck.Add(new CacheInfo { CultureName = cultureName, TenantId = CORE });

                // The cache representing the tenant translations in that specific culture
                if (_tenantIdProvider.HasTenantId())
                {
                    string tenantId = _tenantIdProvider.GetTenantId().ToString();
                    cachesToCheck.Add(new CacheInfo { CultureName = cultureName, TenantId = tenantId });
                }
            }

            // Determine which caches represent a stale caches and need refreshing
            List<CacheInfo> staleCaches = new List<CacheInfo>();
            foreach (var cacheToCheck in cachesToCheck)
            {
                // Retrieve the timestamps from the distributed cache
                var cacheKey = CacheKey(cacheToCheck.CultureName, cacheToCheck.TenantId);
                var distVersion = _distributedCache.GetString(cacheKey);

                // If the distributed cache is blank or invalid, set it to NOW
                if (distVersion == null)
                {
                    // Either the cache was flushed, or has been illegally tampered with,
                    // simply reset the cache to NOW, to invalidate all local caches
                    distVersion = Guid.NewGuid().ToString();
                    _distributedCache.SetString(cacheKey, distVersion, GetDistributedCacheOptions());

                    // NOTE: This should work fine with concurrency, the worst that could happen is
                    // that if multiple nodes independently find that the distributed cache is blank
                    // then one of the nodes may overwrite the version inserted by the other nodes
                    // and then for those other nodes, the freshly retrieved translations may end up
                    // being fetched once more in the next request, since their local cache will have
                    // a different version
                }

                _localCache.TryGetValue(cacheKey, out LocalCacheItem localCacheItem);

                // If the local cache is blank or outdated, grab a fresh copy from the DB
                if (localCacheItem == null || localCacheItem.Version != distVersion)
                {
                    // Remember the version from the distributed cache
                    cacheToCheck.LatestVersion = distVersion;
                    staleCaches.Add(cacheToCheck);
                }
            }

            ////// Efficiently retrieve a fresh list of translations for all stale caches
            List<TranslationDTO> allFreshTranslations = new List<TranslationDTO>();

            // First pass for core translations, those are retrieved from LocalizationContext
            var coreStaleCaches = staleCaches.Where(e => e.TenantId == CORE);
            if (coreStaleCaches.Any())
            {
                var staleCacheKeys = coreStaleCaches.Select(e => e.CultureName);
                using (var scope = _serviceProvider.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<LocalizationContext>();
                    var freshTranslations = ctx.CoreTranslations
                        .Where(e => (e.Tier == Constants.Server || e.Tier == Constants.Shared) && staleCacheKeys.Contains(e.Culture))
                        .ToList()
                        .Select(e => new TranslationDTO { CacheKey = CacheKey(e.Culture, CORE), Name = e.Name, Value = e.Value });

                    allFreshTranslations.AddRange(freshTranslations);
                }
            }

            // Second pass for tenant translations, those are retrieved from ApplicationContext
            var tenantStaleCaches = staleCaches.Where(e => e.TenantId != CORE);
            if (tenantStaleCaches.Any())
            {
                var staleCacheKeys = tenantStaleCaches.Select(e => e.CultureName);
                using (var scope = _serviceProvider.CreateScope())
                {

                    string tenantId = _tenantIdProvider.GetTenantId().ToString();
                    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    var freshTranslations = ctx.Translations
                        .Where(e => (e.Tier == Constants.Server || e.Tier == Constants.Shared) && staleCacheKeys.Contains(e.Culture))
                        .ToList()
                        .Select(e => new TranslationDTO { CacheKey = CacheKey(e.Culture, tenantId), Name = e.Name, Value = e.Value });

                    allFreshTranslations.AddRange(freshTranslations);
                }
            }

            // Arrange all freshly retrieved translations in a dictionary
            Dictionary<string, Dictionary<string, string>> allFreshTranslationsDic = allFreshTranslations
                .GroupBy(e => e.CacheKey)
                .ToDictionary(g => g.Key, g => g.ToDictionary(e => e.Name, e => e.Value));

            // Update the stale caches with the fresh translations
            foreach (var staleCache in staleCaches)
            {
                // Get the cache key
                var cacheKey = CacheKey(staleCache.CultureName, staleCache.TenantId);

                // Prepare the translations in a dictionary, even an empty one if no list came from SQL
                Dictionary<string, string> translations = new Dictionary<string, string>();
                if (allFreshTranslationsDic.ContainsKey(cacheKey))
                {
                    translations = allFreshTranslationsDic[cacheKey];
                }

                // Set the local cache
                _localCache[cacheKey] = new LocalCacheItem
                {
                    Version = staleCache.LatestVersion,
                    Translations = translations
                };
            }
        }

        /// <summary>
        /// Marks the entry in the distributed cache 
        /// </summary>
        /// <param name="cultureName"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public async Task InvalidateCacheAsync(string cultureName, int? tenantId = null)
        {
            var cacheKey = CacheKey(cultureName, tenantId?.ToString() ?? CORE);
            var value = Guid.NewGuid().ToString();
            await _distributedCache.SetStringAsync(cacheKey, value, GetDistributedCacheOptions());
        }

        /// <summary>
        /// Creates the distributed cache options, and sets the absolute expiry time
        /// </summary>
        /// <returns></returns>
        private DistributedCacheEntryOptions GetDistributedCacheOptions()
        {
            var opt = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddDays(DISTRIBUTED_CACHE_EXPIRATION_DAYS),
                SlidingExpiration = null
            };

            return opt;
        }

        /// <summary>
        /// Generates the keys used in local and distributed caches
        /// based on the culture and the tenant Id
        /// </summary>
        /// <param name="cultureName"></param>
        /// <param name="tenantId"></param>
        /// <returns>The key to be used in the local and distributed cache</returns>
        private string CacheKey(string cultureName, string tenantId)
            => $"localization:{tenantId}:{cultureName}";

        /// <summary>
        /// Simple DTO to encapsulte an entry in the local cache
        /// </summary>
        private class LocalCacheItem
        {
            public string Version { get; set; }
            public Dictionary<string, string> Translations { get; set; }
        }

        /// <summary>
        /// Simple DTO to temporarily store translations that are freshly retrieved
        /// from SQL
        /// </summary>
        private class TranslationDTO
        {
            public string CacheKey { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// Simple DTO to temporarily represent a stale cache in the cache refresh logic
        /// </summary>
        private class CacheInfo
        {
            public string LatestVersion { get; set; }
            public string TenantId { get; set; }
            public string CultureName { get; set; }
        }
    }
}

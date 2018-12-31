using BSharp.Data;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SqlStringLocalizerFactory(
            IConfiguration config,
            IDistributedCache distributedCache,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _config = config.GetSection("Localization");
            _distributedCache = distributedCache;
            _serviceProvider = serviceProvider;
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
            return new SqlStringLocalizer(this);
        }

        public CascadingTranslations GetTranslationsForCurrentCulture()
        {
            string defaultUICulture = _config["DefaultUICulture"];
            string specificUICulture = CultureInfo.CurrentUICulture.Name;
            string neutralUICulture = CultureInfo.CurrentUICulture.IsNeutralCulture ? specificUICulture : CultureInfo.CurrentUICulture.Parent.Name;

            // We refresh the cache once per request, and we track this using a flag in the HTTP Context object
            var isUpdated =  (bool?) (_httpContextAccessor.HttpContext == null ? false : _httpContextAccessor.HttpContext.Items[HTTP_CONTEXT_FLAG_NAME]);
            if (!(isUpdated ?? false))
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

            _localCache.TryGetValue(CacheKey(specificUICulture), out LocalCacheItem specificCache);
            _localCache.TryGetValue(CacheKey(neutralUICulture), out LocalCacheItem neutralCache);
            _localCache.TryGetValue(CacheKey(defaultUICulture), out LocalCacheItem defaultCache);

            var result = new CascadingTranslations
            {
                CultureName = specificUICulture,

                SpecificTranslations = specificCache?.Translations,
                NeutralTranslations = neutralCache?.Translations,
                DefaultTranslations = defaultCache?.Translations,
            };

            // Return the translations
            return result;
        }

        /// <summary>
        /// Checks the local cache timestamp against the distributed cache timestamp if the local timestamp
        /// is blank or old than the distributed cache's, it updates it with a fresh copy from the database, 
        /// it also updates it if the timestamp in the distributed cache is blank or invalid
        /// </summary>
        private void EnsureFreshnessOfCaches(params string[] cultureNames)
        {
            // Determine which caches represent a stale caches and need refreshing
            List<CacheInfo> staleCaches = new List<CacheInfo>();
            foreach (var cultureName in cultureNames)
            {
                // Retrieve the timestamps from the distributed cache
                var cacheKey = CacheKey(cultureName);
                var distVersion = _distributedCache.GetString(cacheKey);

                // If the distributed cache is blank or invalid, set it to a new version
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
                    staleCaches.Add(new CacheInfo
                    {
                        LatestVersion = distVersion,
                        CultureName = cultureName
                    });
                }
            }

            ////// Efficiently retrieve a fresh list of translations for all stale caches
            Dictionary<string, Dictionary<string, string>> freshTranslations = new Dictionary<string, Dictionary<string, string>>();

            // Get translations from AdminContext
            if (staleCaches.Any())
            {
                var staleCacheKeys = staleCaches.Select(e => e.CultureName);
                using (var scope = _serviceProvider.CreateScope())
                {
                    using (var ctx = scope.ServiceProvider.GetRequiredService<AdminContext>())
                    {
                        var freshTranslationsQuery = from e in ctx.Translations
                                                     where (e.Tier == Constants.Server || e.Tier == Constants.Shared) && staleCacheKeys.Contains(e.CultureId)
                                                     group e by e.CultureId into g
                                                     select g;

                        freshTranslations = freshTranslationsQuery.AsNoTracking().ToDictionary(
                            g => CacheKey(g.Key),
                            g => g.ToDictionary(e => e.Name, e => e.Value));
                    }
                }
            }

            // Update the stale caches with the fresh translations
            foreach (var staleCache in staleCaches)
            {
                // Get the cache key
                var cacheKey = CacheKey(staleCache.CultureName);

                // Prepare the translations in a dictionary, even an empty one if no list came from SQL
                Dictionary<string, string> translations = new Dictionary<string, string>();
                if (freshTranslations.ContainsKey(cacheKey))
                {
                    translations = freshTranslations[cacheKey];
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
        public async Task InvalidateCacheAsync(string cultureName)
        {
            var cacheKey = CacheKey(cultureName);
            var newVersion = Guid.NewGuid().ToString();
            await _distributedCache.SetStringAsync(cacheKey, newVersion, GetDistributedCacheOptions());
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
        /// based on the culture
        /// </summary>
        /// <returns>The key to be used in the local and distributed cache</returns>
        private string CacheKey(string cultureName)
            => $"localization:{cultureName}";

        /// <summary>
        /// Simple DTO to encapsulate an entry in the local cache
        /// </summary>
        private class LocalCacheItem
        {
            public string Version { get; set; }
            public Dictionary<string, string> Translations { get; set; }
        }

        /// <summary>
        /// Simple DTO to temporarily represent a stale cache in the cache refresh logic
        /// </summary>
        private class CacheInfo
        {
            public string LatestVersion { get; set; }
            public string CultureName { get; set; }
        }
    }
}

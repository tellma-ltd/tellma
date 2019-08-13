using BSharp.Controllers.Dto;
using BSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace BSharp.Services.SqlLocalization
{
    /// <summary>
    /// Relies on the translations stored in a SQL server database, this implementation uses 
    /// local memory caching to achieve a high degree of performance
    /// </summary>
    public class SqlStringLocalizerFactory : ISqlStringLocalizerFactory
    {
        /// <summary>
        /// Every culture (e.g. 'en-US') has its own independent lock to reduce thread starvation
        /// </summary>
        private readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _locks
            = new ConcurrentDictionary<string, ReaderWriterLockSlim>();

        /// <summary>
        /// The cache storing the translations, we don't rely on <see cref="IMemoryCache"/> since it evicts
        /// entries under memory pressure or after timeout such that you can't retrieve them again, instead
        /// what we need to do often is keep the old copy and compare its version with the database instead
        /// loading the entire translations table again, if the version matches we simply flag the entry as
        /// fresh again
        /// </summary>
        private readonly Dictionary<string, CacheEntry> _translations =
            new Dictionary<string, CacheEntry>();

        /// <summary>
        /// For retrieving the AdminContext, which we cannot resolve from the constructor since this service
        /// is a singleton and the context is scoped
        /// </summary>
        private readonly IServiceProvider _servicsProvider;

        /// <summary>
        /// The configuration that are relevant to SqlStringLocalizerFactory as per the options pattern
        /// </summary>
        private readonly SqlLocalizationConfiguration _config;

        public SqlStringLocalizerFactory(IServiceProvider servicsProvider,
            IOptions<SqlLocalizationConfiguration> options)
        {
            _servicsProvider = servicsProvider;
            _config = options.Value;
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

        public string Localize(string name, CultureInfo culture)
        {
            // (1) Prepare the list of cultures in their fallback order:
            // specific culture -> neutral culture -> system default culture.
            // For example { 'ar-SA', 'ar', 'en' }
            // Here we add the specific culture
            List<string> orderedCultureNames = new List<string> { culture.Name };

            if (!culture.IsNeutralCulture)
            {
                // Then the neutral culture (if needed)
                orderedCultureNames.Add(culture.Parent.Name);
            }

            string defaultCultureName = _config.DefaultUICulture;
            if (!orderedCultureNames.Contains(defaultCultureName))
            {
                // Then the default system culture (if needed)
                orderedCultureNames.Add(defaultCultureName);
            }

            // (2) Go over the cultures in order until you find a translation
            foreach (var cultureName in orderedCultureNames)
            {
                var cultureLock = _locks.GetOrAdd(cultureName, new ReaderWriterLockSlim());
                CacheEntry entry = null;
                cultureLock.EnterReadLock();
                try
                {
                    _translations.TryGetValue(cultureName, out entry);
                }
                finally
                {
                    cultureLock.ExitReadLock();
                }

                // In case of a cache miss, do the needful
                if (entry == null || entry.IsExpired(_config.CacheExpirationMinutes))
                {
                    cultureLock.EnterWriteLock();
                    try
                    {
                        // To prevent a race condition we check once again inside the write lock that it's a cache miss
                        _translations.TryGetValue(cultureName, out entry);
                        if (entry == null || entry.IsExpired(_config.CacheExpirationMinutes))
                        {
                            // At this point it's a definitely a cache miss => refresh the cache from the source database
                            using (var scope = _servicsProvider.CreateScope())
                            {
                                using (var db = scope.ServiceProvider.GetService<AdminContext>())
                                {
                                    // Load the version from the dB
                                    var dbVersion = LoadTranslationsVersion(cultureName, db);

                                    // If the entry is already loaded before but simply expired, don't immediately load the entire table
                                    // again, first compare the version string of the cache with that of the DB, and if they match
                                    // flag the entry in the cache as fresh
                                    if (entry == null || entry.Version != dbVersion)
                                    {
                                        // IF this is the first query of if the versions don't match create new a cache entry

                                        // Load the version + translations from the DB
                                        var freshTranslations = LoadTranslations(cultureName, db);

                                        // Create a new entry
                                        entry = new CacheEntry
                                        {
                                            Translations = freshTranslations,
                                            Version = dbVersion
                                        };

                                        _translations[cultureName] = entry;
                                    }

                                    entry.MarkAsFresh();
                                }
                            }
                        }
                    }
                    finally
                    {
                        cultureLock.ExitWriteLock();
                    }
                }

                // If a translation is found return it
                if (entry.Translations.ContainsKey(name))
                {
                    return entry.Translations[name].Value;
                }
            }

            // (3) If no translation is found: Forgiveness is a virtue, return the resource name as is
            return name;
        }

        private Dictionary<string, TranslationInfo> LoadTranslations(string cultureName, AdminContext db)
        {
            var translations = db.Translations.AsNoTracking()
                .Where(e => e.CultureId == cultureName)
                .ToDictionary(e => e.Name, e => new TranslationInfo
                {
                    Value = e.Value,
                    Tier = e.Tier
                });

            return translations;
        }

        private string LoadTranslationsVersion(string cultureName, AdminContext db)
        {
            var dbCulture = db.Cultures.FirstOrDefault(e => e.Id == cultureName);
            var version = dbCulture?.TranslationsVersion ?? Guid.Empty;

            return version.ToString();
        }

        public void InvalidateCache(string cultureName)
        {
            var cultureLock = _locks.GetOrAdd(cultureName, new ReaderWriterLockSlim());
            cultureLock.EnterWriteLock();
            try
            {
                _translations.TryGetValue(cultureName, out CacheEntry entry);
                if (entry != null)
                {
                    // The next call for a localized string will force a database refresh
                    entry.Version = Guid.NewGuid().ToString();
                }
            }
            finally
            {
                cultureLock.ExitWriteLock();
            }
        }

        public DataWithVersion<Dictionary<string, string>> GetTranslations(string cultureName, params string[] tiers)
        {
            EnsureFreshnessOfCache(cultureName);

            // Retrieve the translations pertaining to the specified tiers
            var cultureLock = _locks.GetOrAdd(cultureName, new ReaderWriterLockSlim());
            cultureLock.EnterReadLock();
            try
            {
                _translations.TryGetValue(cultureName, out CacheEntry entry);

                var version = entry.Version;
                var data = entry.Translations
                    .Where(e => tiers == null || tiers.Contains(e.Value.Tier))
                    .ToDictionary(e => e.Key, e => e.Value.Value);

                return new DataWithVersion<Dictionary<string, string>>
                {
                    Version = version,
                    Data = data
                };
            }
            finally
            {
                cultureLock.ExitReadLock();
            }
        }

        public bool IsFresh(string cultureName, string version)
        {
            EnsureFreshnessOfCache(cultureName);

            // Retrieve the translations pertaining to the specified tiers
            var cultureLock = _locks.GetOrAdd(cultureName, new ReaderWriterLockSlim());
            cultureLock.EnterReadLock();
            try
            {
                _translations.TryGetValue(cultureName, out CacheEntry entry);
                return entry.Version == version;
            }
            finally
            {
                cultureLock.ExitReadLock();
            }
        }

        private void EnsureFreshnessOfCache(string cultureName)
        {
            // Ensures a fresh cache (within the expiry timeout)
            Localize("AppName", new CultureInfo(cultureName));
        }

        private class CacheEntry
        {
            public string Version { get; set; }
            public DateTimeOffset LastChecked { get; set; } = DateTimeOffset.Now;
            public Dictionary<string, TranslationInfo> Translations { get; set; } = new Dictionary<string, TranslationInfo>();

            /// <summary>
            /// Checks if <see cref="LastChecked"/> is more minutes ago than the provider value
            /// </summary>
            public bool IsExpired(int expirationMinutes)
            {
                var isExpired = (DateTimeOffset.Now - LastChecked).TotalMinutes >= expirationMinutes;
                return isExpired;
            }

            /// <summary>
            /// Updates <see cref="LastChecked"/> to current time
            /// </summary>
            public void MarkAsFresh()
            {
                LastChecked = DateTimeOffset.Now;
            }
        }

        private class TranslationInfo
        {
            public string Value { get; set; }
            public string Tier { get; set; }
        }
    }
}

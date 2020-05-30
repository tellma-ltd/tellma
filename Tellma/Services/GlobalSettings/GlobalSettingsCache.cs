using Tellma.Controllers.Dto;
using Tellma.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading;

namespace Tellma.Services.GlobalSettings
{
    public class GlobalSettingsCache : IGlobalSettingsCache
    {
        // The cache contents
        private GlobalSettingsForClient _cache = null;
        private DateTimeOffset _lastChecked = DateTimeOffset.MinValue;
        private string _version = Guid.Empty.ToString();

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly IServiceProvider _serviceProvider;
        private readonly GlobalSettingsCacheOptions _config;

        public GlobalSettingsCache(IServiceProvider serviceProvider, IOptions<GlobalSettingsCacheOptions> options)
        {
            _serviceProvider = serviceProvider;
            _config = options.Value;
        }

        public Versioned<GlobalSettingsForClient> GetGlobalSettings()
        {
            GlobalSettingsForClient result = null;
            _lock.EnterReadLock();
            try
            {
                result = _cache;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (result == null || CacheIsExpired())
            {
                _lock.EnterWriteLock();
                try
                {
                    result = _cache;
                    if (result == null || CacheIsExpired())
                    {
                        using var scope = _serviceProvider.CreateScope();
                        using var db = scope.ServiceProvider.GetRequiredService<AdminRepository>();
                        var dbVersion = LoadGlobalSettingsVersion(db);

                        if (result == null || _version != dbVersion)
                        {
                            _cache = LoadGlobalSettings(db);
                            result = _cache;
                        }

                        _version = dbVersion;
                        _lastChecked = DateTimeOffset.Now;
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            return new Versioned<GlobalSettingsForClient>
            {
                Data = result,
                Version = _version
            };
        }

        private string LoadGlobalSettingsVersion(AdminRepository _)
        {
            //var dbSettings = db.GlobalSettings.FirstOrDefault();
            //var version = dbSettings?.SettingsVersion ?? Guid.Empty;

            var version = Guid.Empty;
            return version.ToString();
        }

        private GlobalSettingsForClient LoadGlobalSettings(AdminRepository _)
        {
            var result = new GlobalSettingsForClient
            {
            };

            return result;
        }

        public bool IsFresh(string version)
        {
            _lock.EnterReadLock();
            try
            {
                return _version == version;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void InvalidateCache()
        {
            _lock.EnterWriteLock();
            try
            {
                _version = Guid.NewGuid().ToString();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void EnsureFreshnessOfCache()
        {
            // Ensures a fresh cache (within the expiry timeout)
            GetGlobalSettings();
        }

        private bool CacheIsExpired()
        {
            return (DateTimeOffset.Now - _lastChecked).TotalMinutes > _config.CacheExpirationMinutes;
        }
    }
}

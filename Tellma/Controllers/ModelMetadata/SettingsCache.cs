using Tellma.Controllers.Dto;
using Tellma.Services.MultiTenancy;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Tellma.Controllers
{
    public class SettingsCache : ISettingsCache
    {
        /// <summary>
        /// This efficient lock prevents concurrency issues when updating the cache.
        /// Technically we should have a lock per database ID, but this will complicate the code for little performance benefit
        /// </summary>
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Mapping from database ID to its <see cref="SettingsForClient"/>
        /// </summary>
        private static readonly ConcurrentDictionary<int, DataWithVersion<SettingsForClient>> _cache 
            = new ConcurrentDictionary<int, DataWithVersion<SettingsForClient>>();

        private readonly ITenantIdAccessor _tenantIdAccessor;

        public SettingsCache(ITenantIdAccessor tenantIdAccessor)
        {
            _tenantIdAccessor = tenantIdAccessor;
        }

        /// <summary>
        /// Implementation of <see cref="ISettingsCache"/>
        /// </summary>
        public DataWithVersion<SettingsForClient> GetSettingsIfCached(int databaseId)
        {
            _lock.EnterReadLock();
            try
            {
                _cache.TryGetValue(databaseId, out DataWithVersion<SettingsForClient> Settings);
                return Settings;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public DataWithVersion<SettingsForClient> GetCurrentSettingsIfCached()
        {
            int databaseId = _tenantIdAccessor.GetTenantId();
            return GetSettingsIfCached(databaseId);
        }

        /// <summary>
        /// Implementation of <see cref="ISettingsCache"/>
        /// </summary>
        public void SetSettings(int databaseId, DataWithVersion<SettingsForClient> Settings)
        {
            if (databaseId == 0)
            {
                throw new ArgumentException($"{nameof(databaseId)} must be provided");
            }

            if (Settings is null)
            {
                throw new ArgumentNullException(nameof(Settings));
            }

            _cache.AddOrUpdate(databaseId, Settings, (i, d) => Settings);
        }
    }
}

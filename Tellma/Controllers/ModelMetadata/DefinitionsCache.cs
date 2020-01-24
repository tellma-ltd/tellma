using Tellma.Controllers.Dto;
using Tellma.Services.MultiTenancy;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Tellma.Controllers
{
    public class DefinitionsCache : IDefinitionsCache
    {
        /// <summary>
        /// This efficient lock prevents concurrency issues when updating the cache.
        /// Technically we should have a lock per database ID, but this will complicate the code for little performance benefit
        /// </summary>
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Mapping from database ID to its <see cref="DefinitionsForClient"/>
        /// </summary>
        private static readonly ConcurrentDictionary<int, DataWithVersion<DefinitionsForClient>> _cache 
            = new ConcurrentDictionary<int, DataWithVersion<DefinitionsForClient>>();

        private readonly ITenantIdAccessor _tenantIdAccessor;

        public DefinitionsCache(ITenantIdAccessor tenantIdAccessor)
        {
            _tenantIdAccessor = tenantIdAccessor;
        }

        /// <summary>
        /// Implementation of <see cref="IDefinitionsCache"/>
        /// </summary>
        public DataWithVersion<DefinitionsForClient> GetDefinitionsIfCached(int databaseId)
        {
            _lock.EnterReadLock();
            try
            {
                _cache.TryGetValue(databaseId, out DataWithVersion<DefinitionsForClient> definitions);
                return definitions;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public DataWithVersion<DefinitionsForClient> GetCurrentDefinitionsIfCached()
        {
            int databaseId = _tenantIdAccessor.GetTenantId();
            return GetDefinitionsIfCached(databaseId);
        }

        /// <summary>
        /// Implementation of <see cref="IDefinitionsCache"/>
        /// </summary>
        public void SetDefinitions(int databaseId, DataWithVersion<DefinitionsForClient> definitions)
        {
            if (databaseId == 0)
            {
                throw new ArgumentException($"{nameof(databaseId)} must be provided");
            }

            if (definitions is null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            _cache.AddOrUpdate(databaseId, definitions, (i, d) => definitions);
        }
    }
}

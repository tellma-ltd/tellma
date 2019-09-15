using BSharp.Data;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace BSharp.Controllers
{
    /// <summary>
    /// Some entities have their metadata dynamically constructed from database configuration (Settings, definitions).
    /// This service caches the metadata in memory since it is frequently queried
    /// </summary>
    public interface IDatabaseModelMetadataCache
    {
        /// <summary>
        /// Returns the cached model metadata associated with the given database ID or null if non exist
        /// </summary>
        DatabaseModelMetadata GetModelMetadataIfCached(int databaseId);

        /// <summary>
        /// Sets the cached model metadata associated with the given database ID
        /// </summary>
        void SetModelMetadata(int databaseId, DatabaseModelMetadata modelMetadata);
    }

    public class DatabaseModelMetadataCache : IDatabaseModelMetadataCache
    {
        /// <summary>
        /// This efficient lock prevents concurrency issues when updating the cache.
        /// Technically we should have a lock per database ID, but this will complicate the code for little performance benefit
        /// </summary>
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Mapping from database ID to all its <see cref="DatabaseModelMetadata"/>
        /// </summary>
        private static readonly ConcurrentDictionary<int, DatabaseModelMetadata> _cache = new ConcurrentDictionary<int, DatabaseModelMetadata>();

        /// <summary>
        /// Implementation of <see cref="IDatabaseModelMetadataCache"/>
        /// </summary>
        public DatabaseModelMetadata GetModelMetadataIfCached(int databaseId)
        {
            _lock.EnterReadLock();
            try
            {
                _cache.TryGetValue(databaseId, out DatabaseModelMetadata modelMetadata);
                return modelMetadata;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Implementation of <see cref="IDatabaseModelMetadataCache"/>
        /// </summary>
        public void SetModelMetadata(int databaseId, DatabaseModelMetadata modelMetadata)
        {
            if (databaseId == 0)
            {
                throw new ArgumentException($"{nameof(databaseId)} must be provided");
            }

            if (modelMetadata is null)
            {
                throw new ArgumentNullException(nameof(modelMetadata));
            }

            _cache.AddOrUpdate(databaseId, modelMetadata, (i, d) => modelMetadata);
        }
    }
}

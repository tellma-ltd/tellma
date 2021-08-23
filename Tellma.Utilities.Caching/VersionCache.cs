using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Utilities.Caching
{
    public abstract class VersionCache<TKey, TData>
    {
        private readonly ConcurrentDictionary<TKey, CacheEntry> _cache = new();

        public async Task<(TData data, string version)> GetData(TKey key, string version, CancellationToken cancellation)
        {
            var entry = _cache.GetOrAdd(key, _ => new CacheEntry());
            if (entry.Version != version)
            {
                // Cache miss => we need to refresh the cache.
                // Use the semaphore to make sure only one thread is
                // refreshing the cache while the others await.
                await entry.Semaphore.WaitAsync(cancellation);
                try
                {
                    // A second OCD-check inside the semaphore block
                    if (entry.Version != version)
                    {
                        // Load from source
                        var (freshData, freshVersion) = await GetDataFromSource(key, cancellation);
                        if (entry.Version != freshVersion)
                        {
                            entry.Data = freshData;
                            entry.Version = freshVersion;
                        }
                    }
                }
                finally
                {
                    // Very important
                    entry.Semaphore.Release();
                }
            }

            return (entry.Data, entry.Version);
        }

        protected abstract Task<(TData data, string version)> GetDataFromSource(TKey key, CancellationToken cancellation);

        private class CacheEntry
        {
            public SemaphoreSlim Semaphore { get; } = new(initialCount: 1);

            public string Version { get; set; } = Guid.NewGuid().ToString();

            public TData Data { get; set; }
        }
    }
}

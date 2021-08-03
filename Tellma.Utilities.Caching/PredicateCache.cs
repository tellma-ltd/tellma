using System;
using System.Collections.Concurrent;

namespace Tellma.Utilities.Caching
{
    public abstract class PredicateCache<TKey, TData>
    {
        private readonly ConcurrentDictionary<TKey, TData> _cache = new();

        public TData GetData(TKey key, Predicate<TData> isFresh)
        {
            var entry = _cache.GetOrAdd(key, RecreateData);
            if (!isFresh(entry))
            {
                _cache.TryRemove(key, out _);
                return GetData(key, _ => true);
            }
            else
            {
                return entry;
            }
        }

        protected abstract TData RecreateData(TKey key);
    }
}

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Utilities.Sharding
{
    /// <summary>
    /// The default implementation of the <see cref="IShardResolver"/>.
    /// </summary>
    public class CachingShardResolver : IShardResolver
    {
        // This efficient semaphore prevents concurrency issues when updating the cache
        private static readonly SemaphoreSlim _semaphore = new(1);

        private readonly IConnectionInfoLoader _resolver;
        private readonly IMemoryCache _cache;
        private readonly ShardResolverOptions _options;

        public CachingShardResolver(IConnectionInfoLoader resolver, IMemoryCache cache, IOptions<ShardResolverOptions> options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = options.Value;
        }

        /// <summary>
        /// Get the number of minutes after which the connection string cache is considered expired.
        /// </summary>
        private int CacheExpirationInMinutes => _options?.ShardResolverCacheExpirationMinutes ?? 120; // Default = 2 hours

        private int ConnectionTimeoutInSeconds => _options?.ConnectionStringsTimeoutInSeconds ?? 15 * 16;

        /// <summary>
        /// Get the key to use when storing the application database connection in the <see cref="IMemoryCache"/>
        /// </summary>
        private static string CacheKey(int databaseId) => $"Sharding:{databaseId}";

        /// <summary>
        /// Implementation of <see cref="IShardResolver.GetConnectionString(int, CancellationToken)"/>.
        /// </summary>
        public async Task<string> GetConnectionString(int databaseId, CancellationToken cancellation)
        {
            // Step (1) Try retrieving the connection string from the cache
            if (_cache.TryGetValue(CacheKey(databaseId), out string shardConnString))
            {
                return shardConnString;
            }

            // Step (2) if step 1 was a miss, request the semaphore to guarantee only one thread can
            // access the try-block, in there retrieve the conn string from the resolver and update the cache
            else
            {
                // Only one thread at a time can enter the next block of code
                await _semaphore.WaitAsync(cancellation);
                try
                {
                    // To avoid a race-condition causing multiple threads to populate the cache in parallel immediately after they all 
                    // have a cache miss, here we check the cache again inside the semaphore block
                    if (_cache.TryGetValue(CacheKey(databaseId), out shardConnString))
                    {
                        return shardConnString;
                    }
                    else // A miss for sure
                    {
                        // (1) retrieve the database info of this database Id from the registered Resolver
                        var connInfo = await _resolver.Load(databaseId, cancellation);

                        // (2) Prepare the connection string
                        if (IsValid(connInfo))
                        {
                            var shardConnStringBuilder = new SqlConnectionStringBuilder
                            {
                                DataSource = connInfo.ServerName,
                                InitialCatalog = connInfo.DatabaseName,
                                UserID = connInfo.UserName,
                                Password = connInfo.Password,
                                IntegratedSecurity = connInfo.IsWindowsAuth,
                                PersistSecurityInfo = false,
                                MultipleActiveResultSets = true,
                                ConnectTimeout = ConnectionTimeoutInSeconds // Increase the SQL server timeout to 15 minutes (web server timeout is 15.25 minutes)
                            };

                            shardConnString = shardConnStringBuilder.ConnectionString;
                        }
                        else
                        {
                            // Invalid info
                            shardConnString = null;
                        }

                        // Set the cache, with an expiry
                        var expiryTime = DateTimeOffset.Now.AddMinutes(CacheExpirationInMinutes);
                        _cache.Set(CacheKey(databaseId), shardConnString, expiryTime);

                        // NOTE: Shard connection strings are very frequently read, yet very rarely if never updated so we have decided to rely
                        // only on cache expiry to keep the cache fresh (2h by default), so if you change an application database name or credentials,
                        // you need to wait those 2 hours before all caches are updated, this is the best compromise.
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return shardConnString;
        }

        private static bool IsValid(DatabaseConnectionInfo info)
        {
            return !string.IsNullOrWhiteSpace(info.ServerName) &&
                !string.IsNullOrWhiteSpace(info.DatabaseName);
        }
    }
}

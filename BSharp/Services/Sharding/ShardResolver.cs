using BSharp.Data;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace BSharp.Services.Sharding
{
    /// <summary>
    /// Default implementation of <see cref="IShardResolver"/> which retrieves the
    /// connection strings from a shard manager database, and caches them for faster access
    /// </summary>
    public class ShardResolver : IShardResolver
    {
        public const string SHARD_MANAGER_PLACEHOLDER = "<ShardManager>";
        public const string PASSWORDS_CONFIG_SECTION = "Passwords";
        private const string SHARD_RESOLVER_EXPIRATION_CONFIG_KEY = "ShardResolverCacheExpirationMinutes";

        // This efficient lock prevents concurrency issues when updating the cache
        private static ReaderWriterLockSlim _shardingLock = new ReaderWriterLockSlim();

        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public ShardResolver(ITenantIdProvider tenantIdProvider,
            IServiceProvider serviceProvider, IMemoryCache cache, IConfiguration config)
        {
            _tenantIdProvider = tenantIdProvider;
            _serviceProvider = serviceProvider;
            _cache = cache;
            _config = config;
        }

        public string GetShardConnectionString()
        {
            // When applying the migrations in Program.cs while running the 
            // solution in development, it is convenient to have a default 
            // connection string that doesn't depend on tenant Id
            if (!_tenantIdProvider.HasTenantId())
            {
                return _config.GetConnectionString(Constants.ManagerConnection);
            }

            string shardConnString = null;
            int tenantId = _tenantIdProvider.GetTenantId().Value;

            // Step (1) retrieve the conn string from the cache inside a READ lock
            _shardingLock.EnterReadLock();
            try
            {
                _cache.TryGetValue(CacheKey(tenantId), out shardConnString);
            }
            finally
            {
                _shardingLock.ExitReadLock();
            }

            // Step (2) if step (1) was a miss, enter inside a WRITE lock, retrieve the conn string from the source and update the cache
            if (shardConnString == null)
            {
                _shardingLock.EnterWriteLock();
                try
                {
                    // To avoid a race-condition causing multiple threads to populate the cache in parallel immediately after they all 
                    // have a cache miss inside the previous READ lock, here we check the cache again inside the WRITE lock
                    _cache.TryGetValue(CacheKey(tenantId), out shardConnString);
                    if (shardConnString == null)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var ctx = scope.ServiceProvider.GetRequiredService<ManagerContext>();
                            shardConnString = ctx.Tenants.Include(e => e.Shard)
                                .FirstOrDefault(e => e.Id == tenantId)?.Shard?.ConnectionString;
                        }
                        // This is a catastrophic error, should not happen in theory
                        if (string.IsNullOrWhiteSpace(shardConnString))
                        {
                            throw new InvalidOperationException($"The sharding route for tenant Id {tenantId} is missing");
                        }

                        // There is always one built-in shard that resides in the same DB as the shard manager, the
                        // purpose behind it is to make it easier to do development and also to set-up small instances that do not require sharding 
                        else if (shardConnString == SHARD_MANAGER_PLACEHOLDER)
                        {
                            shardConnString = _config.GetConnectionString(Constants.ManagerConnection);
                        }

                        // ELSE: this is a normal shard
                        else
                        {
                            // For improved security, allow more secure modes of storing shard passwords, other than in the shard manager DB itself
                            // - Mode 1: in a "Passwords" section in a secure configuration provider, and then they are referenced in the conn string by their names
                            // - Mode 2: as being the same password as the shard manager's connection string, which is also stored safely in a configuration provider

                            var shardConnBuilder = new SqlConnectionStringBuilder(shardConnString);
                            if (!string.IsNullOrWhiteSpace(shardConnBuilder.Password))
                            {
                                // If the shard password is specified, and it matches a valid key in the "Passwords" configuration section, use that configuration value instead
                                string configPassword = _config[$"{PASSWORDS_CONFIG_SECTION}:{shardConnBuilder.Password}"];
                                if (!string.IsNullOrWhiteSpace(configPassword))
                                {
                                    shardConnBuilder.Password = configPassword;
                                    shardConnString = shardConnBuilder.ConnectionString;
                                }
                                // ELSE we hope that this is a valid password, or else the connection to the shard will sadly fail.
                            }
                            else
                            {
                                // If the password of the shard is not set but the password of the shard manager is, use the shard manager's
                                string shardManagerConnection = _config.GetConnectionString(Constants.ManagerConnection);
                                var shardManagerConnBuilder = new SqlConnectionStringBuilder(shardManagerConnection);

                                if (!string.IsNullOrWhiteSpace(shardManagerConnBuilder.Password))
                                {
                                    shardConnBuilder.Password = shardManagerConnBuilder.Password;
                                    shardConnString = shardConnBuilder.ConnectionString;
                                }
                                // ELSE we hope that this is windows authentication, or else the connection to the shard will sadly fail.
                            }
                        }

                        // Set the cache, with an expiry
                        var expiryTime = DateTimeOffset.Now.AddMinutes(GetCacheExpirationInMinutes());
                        _cache.Set(CacheKey(tenantId), shardConnString, expiryTime);

                        // NOTE: Sharding routes is a type of data that is very frequently read, yet very rarely if never updated
                        // so we have decided to rely only on cache expiry to keep the cache fresh (2h by default), so if you move a tenant
                        // across shards, you need to wait those 2 hours before all caches are updated. This is the best compromise
                    }
                }
                finally
                {
                    _shardingLock.ExitWriteLock();
                }
            }

            return shardConnString;
        }

        private double GetCacheExpirationInMinutes()
        {
            // Get from the configuration or use a default value
            string key = SHARD_RESOLVER_EXPIRATION_CONFIG_KEY;
            return _config.GetValue(key: key, defaultValue: 120d);
        }

        private string CacheKey(int tenantId) => $"Sharding:{tenantId}";
    }
}

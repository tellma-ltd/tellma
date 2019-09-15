using BSharp.Data;
using BSharp.Services.MultiTenancy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Threading;

namespace BSharp.Services.Sharding
{
    /// <summary>
    /// Default implementation of <see cref="IShardResolver"/> which retrieves the
    /// connection strings from a shard manager database, and caches them for faster access
    /// </summary>
    public class ShardResolver : IShardResolver
    {
        public const string ADMIN_SERVER_PLACEHOLDER = "<AdminServer>";

        // This efficient lock prevents concurrency issues when updating the cache
        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private readonly ITenantIdAccessor _tenantIdProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;
        private readonly ShardResolverOptions _options;
        private readonly AdminRepositoryOptions _adminOptions;

        public ShardResolver(ITenantIdAccessor tenantIdAccessor, IServiceProvider serviceProvider,
            IMemoryCache cache, IOptions<ShardResolverOptions> options, IOptions<AdminRepositoryOptions> adminOptions)
        {
            _tenantIdProvider = tenantIdAccessor;
            _serviceProvider = serviceProvider;
            _cache = cache;
            _adminOptions = adminOptions.Value;
            _options = options.Value;
        }

        public string GetConnectionString(int? tenantId = null)
        {
            string shardConnString = null;
            int databaseId = tenantId ?? _tenantIdProvider.GetTenantId();

            // Step (1) retrieve the conn string from the cache inside a READ lock
            _lock.EnterReadLock();
            try
            {
                _cache.TryGetValue(CacheKey(databaseId), out shardConnString);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // Step (2) if step (1) was a miss, enter inside a WRITE lock, retrieve the conn string from the source and update the cache
            if (shardConnString == null)
            {
                _lock.EnterWriteLock();
                try
                {
                    // To avoid a race-condition causing multiple threads to populate the cache in parallel immediately after they all 
                    // have a cache miss inside the previous READ lock, here we check the cache again inside the WRITE lock
                    _cache.TryGetValue(CacheKey(databaseId), out shardConnString);
                    if (shardConnString == null)
                    {
                        DatabaseConnectionInfo connectionInfo;

                        string serverName = null;
                        string dbName = null;
                        string userName = null;
                        string password = null;
                        bool isWindowsAuth = false;

                        // (1) retrieve the connection info of this database Id
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var repo = scope.ServiceProvider.GetRequiredService<AdminRepository>();
                            connectionInfo = repo.GetDatabaseConnectionInfo(databaseId: databaseId).GetAwaiter().GetResult();

                            dbName = connectionInfo?.DatabaseName;
                        }

                        // This is a catastrophic error, should not happen in theory
                        if (string.IsNullOrWhiteSpace(dbName))
                        {
                            throw new InvalidOperationException($"The sharding route for tenant Id {databaseId} is missing");
                        }

                        // This is the same SQL Server where the admin database resides
                        else if (connectionInfo.ServerName == ADMIN_SERVER_PLACEHOLDER)
                        {
                            // Get the connection string of the manager
                            var shardManagerConnection = _adminOptions.ConnectionString;
                            var shardManagerConnBuilder = new SqlConnectionStringBuilder(shardManagerConnection);

                            // Everything comes from the Admin connection string except the database name
                            serverName = shardManagerConnBuilder.DataSource;
                            userName = shardManagerConnBuilder.UserID;
                            password = shardManagerConnBuilder.Password;

                            isWindowsAuth = shardManagerConnBuilder.IntegratedSecurity;
                        }

                        // ELSE: this is a different SQL Server use the information in ConnectionInfo
                        else
                        {
                            serverName = connectionInfo.ServerName;
                            userName = connectionInfo.UserName;

                            // For better security, there are 2 modes of storing shard passwords:
                            // - Mode 1: in a "Sharding:Passwords" section in a secure configuration provider, and then they are referenced in the DB by their names
                            // - Mode 2: as being the same password as the shard manager's connection string, which is also stored safely in a configuration provider

                            if (!string.IsNullOrWhiteSpace(connectionInfo.PasswordKey))
                            {
                                // If the shard password is specified, and it matches a valid key in the "Passwords" configuration section, use that configuration value instead
                                // string configPassword = _config[$"{PASSWORDS_CONFIG_SECTION}:{shardConnBuilder.Password}"];

                                if (_options?.Passwords != null && _options.Passwords.ContainsKey(connectionInfo.PasswordKey))
                                {
                                    password = _options.Passwords[connectionInfo.PasswordKey];
                                }
                                else
                                {
                                    throw new InvalidOperationException($"The password key '{connectionInfo.PasswordKey}' must be specified in a configuration provider under 'Sharding:Passwords'");
                                }
                            }
                            else
                            {
                                // If the password of the shard is not set but the password of the shard manager is, use the shard manager's
                                string shardManagerConnection = _adminOptions.ConnectionString;
                                var shardManagerConnBuilder = new SqlConnectionStringBuilder(shardManagerConnection);

                                if (!string.IsNullOrWhiteSpace(shardManagerConnBuilder.Password))
                                {
                                    password = shardManagerConnBuilder.Password;
                                }
                                else
                                {
                                    // ELSE we hope that this is windows authentication on a development machine, or else the connection to the shard will sadly fail.
                                    isWindowsAuth = shardManagerConnBuilder.IntegratedSecurity;
                                }
                            }
                        }

                        // (2) Prepare the connection string
                        var shardConnStringBuilder = new SqlConnectionStringBuilder
                        {
                            DataSource = serverName,
                            InitialCatalog = dbName,
                            UserID = userName,
                            Password = password,
                            IntegratedSecurity = isWindowsAuth,
                            PersistSecurityInfo = false,
                            MultipleActiveResultSets = true,
                        };

                        shardConnString = shardConnStringBuilder.ConnectionString;

                        // Set the cache, with an expiry
                        var expiryTime = DateTimeOffset.Now.AddMinutes(GetCacheExpirationInMinutes());
                        _cache.Set(CacheKey(databaseId), shardConnString, expiryTime);

                        // NOTE: Sharding routes is a type of data that is very frequently read, yet very rarely if never updated
                        // so we have decided to rely only on cache expiry to keep the cache fresh (2h by default), so if you move a tenant
                        // across shards, you need to wait those 2 hours before all caches are updated. This is the best compromise
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            return shardConnString;
        }

        private double GetCacheExpirationInMinutes() => _options?.ShardResolverCacheExpirationMinutes ?? 120d;

        private string CacheKey(int tenantId) => $"Sharding:{tenantId}";
    }
}

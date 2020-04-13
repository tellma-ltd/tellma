using Tellma.Data;
using Tellma.Services.MultiTenancy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Services.Sharding
{
    /// <summary>
    /// Default implementation of <see cref="IShardResolver"/> which retrieves the
    /// connection strings from a shard manager database, and caches them for faster access
    /// </summary>
    public class ShardResolver : IShardResolver
    {
        public const string ADMIN_SERVER_PLACEHOLDER = "<AdminServer>";

        // This efficient semaphore prevents concurrency issues when updating the cache
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

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

        public async Task<string> GetConnectionString(int? tenantId = null, CancellationToken cancellation = default)
        {
            int databaseId = tenantId ?? _tenantIdProvider.GetTenantId();

            // Step (1) Try retrieving the connection string from the cache
            if (_cache.TryGetValue(CacheKey(databaseId), out string shardConnString))
            {
                return shardConnString;
            }

            // Step (2) if step 1 was a miss, request the semaphore to guarantee only one thread can
            // access the try-block, in there retrieve the conn string from the DB and update the cache
            else
            {
                // Only one thread at a time can access the next section
                await _semaphore.WaitAsync();
                try
                {
                    // To avoid a race-condition causing multiple threads to populate the cache in parallel immediately after they all 
                    // have a cache miss, here we check the cache again inside the single-threaded block
                    if(_cache.TryGetValue(CacheKey(databaseId), out shardConnString))
                    {
                        return shardConnString;
                    }

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
                            connectionInfo = await repo.GetDatabaseConnectionInfo(databaseId, cancellation);

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
                            var adminConnection = _adminOptions.ConnectionString;
                            var adminConnBuilder = new SqlConnectionStringBuilder(adminConnection);

                            // Everything comes from the Admin connection string except the database name
                            serverName = adminConnBuilder.DataSource;
                            userName = adminConnBuilder.UserID;
                            password = adminConnBuilder.Password;

                            isWindowsAuth = adminConnBuilder.IntegratedSecurity;
                        }

                        // ELSE: this is a different SQL Server use the information in ConnectionInfo
                        else
                        {
                            serverName = connectionInfo.ServerName;
                            userName = connectionInfo.UserName;

                            // For better security, there are 2 modes of storing shard passwords:
                            // - Mode 1: in a "Sharding:Passwords" section in a secure configuration provider, and then they are referenced in the DB by their names
                            // - Mode 2: as being the same password as the admin db's connection string, which is also stored safely in a configuration provider

                            if (!string.IsNullOrWhiteSpace(connectionInfo.PasswordKey))
                            {
                                // If the shard password is specified, and it matches a valid key in the "Passwords" configuration section, use that configuration value instead
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
                                // If the password of the shard is not set but the password of the admin db is, use the admin db's
                                string adminConnection = _adminOptions.ConnectionString;
                                var adminConnBuilder = new SqlConnectionStringBuilder(adminConnection);

                                if (!string.IsNullOrWhiteSpace(adminConnBuilder.Password))
                                {
                                    password = adminConnBuilder.Password;
                                }
                                else
                                {
                                    // ELSE we hope that this is windows authentication on a development machine, or else the connection to the shard will sadly fail.
                                    isWindowsAuth = adminConnBuilder.IntegratedSecurity;
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

                        // NOTE: Shard connection string is a type of data that is very frequently read, yet very rarely if never updated
                        // so we have decided to rely only on cache expiry to keep the cache fresh (2h by default), so if you change a tenant
                        // database name or credentials, you need to wait those 2 hours before all caches are updated. This is the best compromise
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return shardConnString;
        }

        private double GetCacheExpirationInMinutes() => _options?.ShardResolverCacheExpirationMinutes ?? 120d; // Default = 2 hours

        private string CacheKey(int tenantId) => $"Sharding:{tenantId}";
    }
}

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Tellma.Utilities.Sharding
{
    public class CachingShardResolver : ICachingShardResolver
    {
        public const string ADMIN_SERVER_PLACEHOLDER = "<AdminServer>";

        // This efficient semaphore prevents concurrency issues when updating the cache
        private static readonly SemaphoreSlim _semaphore = new(1);


        private readonly IMemoryCache _cache;


        public CachingShardResolver(IShardResolver resolver, IMemoryCache cache, IOptions<ShardResolverOptions> options)
        {
            _repo = repo;
            _cache = cache;
            _adminOptions = adminOptions.Value;
            _options = options.Value;
        }

        public string GetConnectionString(int databaseId)
        {
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
                    if (_cache.TryGetValue(CacheKey(databaseId), out shardConnString))
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
                        using (var _ = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                        {
                            // Suppress any ambient transactions otherwise we might get promoted to a Distributed transaction which is not supported in .NET Core
                            connectionInfo = await _repo.GetDatabaseConnectionInfo(databaseId, cancellation);
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
                            ConnectTimeout = 15 * 60 // Increase the SQL server timeout to 15 minutes (web server timeout is 15.25 minutes)
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

        private static string CacheKey(int databaseId) => $"Sharding:{databaseId}";
    }
}

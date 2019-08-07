using BSharp.EntityModel;
using BSharp.Services.Sharding;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data
{
    public interface IRepository
    {
        RepositoryQuery<T> CreateQuery<T>() where T : Entity;
    }

    /// <summary>
    /// A very thin and lightweight layer around the application database (every tenant
    /// has a single database), it's the entry point of all functionality that requires 
    /// SQL: Tables, Views, Stored Procedures etc.., it contains no logic of its own
    /// </summary>
    public class ApplicationRepository : IDisposable, IRepository
    {
        private readonly IShardResolver _shardResolver;
        private SqlConnection _conn;
        private UserInfo _userInfo;
        private TenantInfo _tenantInfo;

        public ApplicationRepository(IShardResolver shardResolver)
        {
            _shardResolver = shardResolver;
        }

        /// <summary>
        /// By default the <see cref="ApplicationRepository"/> connects to the database corresponding to 
        /// the current tenantId which is retrieved from an injected <see cref="IShardResolver"/>,
        /// this method makes it possible to conncet to a custom connection string instead, 
        /// this is useful when connecting to multiple tenants at the same time to do aggregate reporting for example
        /// </summary>
        public async Task InitConnectionAsync(string connectionString)
        {
            if (_conn != null)
            {
                throw new InvalidOperationException("The connection is already initialized");
            }

            _conn = new SqlConnection(connectionString);
            _conn.Open();

            // Always 
            (_userInfo, _tenantInfo) = await OnConnectAsync(null, null, null, null);
        }

        /// <summary>
        /// Initializes the connection if it is not already initialized
        /// </summary>
        /// <returns>The connection string that was initialized</returns>
        private async Task<SqlConnection> ConnectionAsync()
        {
            if (_conn == null)
            {
                string connString = _shardResolver.GetShardConnectionString();
                await InitConnectionAsync(connString);
            }

            return _conn;
        }

        /// <summary>
        /// Returns the name of the initial catalog from the active connection's connection string
        /// </summary>
        /// <returns></returns>
        private string InitialCatalog()
        {
            if (_conn == null || _conn.ConnectionString == null)
            {
                return null;
            }

            return new SqlConnectionStringBuilder(_conn.ConnectionString).InitialCatalog;
        }

        /// <summary>
        /// Loads a <see cref="UserInfo"/> object from the database, this occurs once per <see cref="ApplicationRepository"/> 
        /// instance, subsequent calls are satisfied from cache
        /// </summary>
        public async Task<UserInfo> GetUserInfoAsync()
        {
            await ConnectionAsync(); // This automatically initializes the user info
            return _userInfo;
        }

        /// <summary>
        /// Loads a <see cref="TenantInfo"/> object from the database, this occurs once per <see cref="ApplicationRepository"/> 
        /// instance, subsequent calls are satisfied from cache
        /// </summary>
        public async Task<TenantInfo> GetTenantInfoAsync()
        {
            await ConnectionAsync(); // This automatically initializes the tenant info
            return _tenantInfo;
        }

        /// <summary>
        /// Creates and returns a new <see cref="ODataQuery{T}"/> object
        /// </summary>
        /// <typeparam name="T">The root type of the query</typeparam>
        public RepositoryQuery<T> CreateQuery<T>() where T : Entity
        {
            throw new NotImplementedException();
        }


        #region Stored Procedures

        public async Task<(UserInfo, TenantInfo)> OnConnectAsync(string externalUserId, string userEmail, string culture, string neutralCulture)
        {
            UserInfo userInfo = null;
            TenantInfo tenantInfo = null;

            using (SqlCommand cmd = _conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.AddWithValue("@ExternalUserId", externalUserId);
                cmd.Parameters.AddWithValue("@UserEmail", userEmail);
                cmd.Parameters.AddWithValue("@Culture", culture);
                cmd.Parameters.AddWithValue("@NeutralCulture", neutralCulture);

                // Command
                cmd.CommandText = @"EXEC [dal].[OnConnect] 
@ExternalUserId = @ExternalUserId, 
@UserEmail      = @UserEmail, 
@Culture        = @Culture, 
@NeutralCulture = @NeutralCulture";

                // Execute and Load
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int i = 0;

                        // The user Info
                        userInfo = new UserInfo
                        {
                            UserId = reader.IsDBNull(i) ? (int?)null : reader.GetInt32(i++),
                            Name = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            Name2 = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            ExternalId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            Email = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            PermissionsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            UserSettingsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                        };

                        // The tenant Info
                        tenantInfo = new TenantInfo
                        {
                            ViewsAndSpecsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            SettingsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            PrimaryLanguageId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            PrimaryLanguageSymbol = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            SecondaryLanguageId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            SecondaryLanguageSymbol = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            TernaryLanguageId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            TernaryLanguageSymbol = reader.IsDBNull(i) ? null : reader.GetString(i++)
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException($"[dal].[OnConnect] did not return any data, InitialCatalog: {InitialCatalog()}, ExternalUserId: {externalUserId}, UserEmail: {userEmail}");
                    }
                }
            }

            return (userInfo, tenantInfo);
        }

        public async Task<IEnumerable<AbstractPermission>> Action_Views__PermissionsAsync(string action, IEnumerable<string> viewIds)
        {
            var result = new List<AbstractPermission>();

            using (SqlCommand cmd = _conn.CreateCommand())
            {
                // Parameters
                var viewIdsTable = RepositoryUtilities.DataTable(viewIds.Select(e => new { Code = e }));
                var viewIdsTvp = new SqlParameter("@ViewIds", viewIdsTable)
                {
                    TypeName = $"dbo.StringList",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(viewIdsTvp);
                cmd.Parameters.AddWithValue("@Action", action);

                cmd.CommandText = @"EXEC [dal].[Action_Views__Permissions]
@Action = @Action,
@ViewIds = @ViewIds
";

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new AbstractPermission
                        {
                            ViewId = reader.GetString(i++),
                            Action = reader.GetString(i++),
                            Criteria = reader.GetString(i++),
                            Mask = reader.GetString(i++)
                        });
                    }
                }
            }

            return result;
        }

        #endregion

        //public async Task<IEnumerable<IdIndex<int>>> MeasurementUnits__SaveAsync(IEnumerable<MeasurementUnit> entities)
        //{
        //    var result = new List<IdIndex<int>>();

        //    SqlConnection conn = await ConnectionAsync();
        //    using (SqlCommand cmd = conn.CreateCommand())
        //    {
        //        // Parameters
        //        cmd.Parameters.AddWithValue("Param1", 33);

        //        // SQL
        //        cmd.CommandText = "EXEC [dal].[MeasurementUnits__Save] @Param1 = @Param1";

        //        // Load the results
        //        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                result.Add(new IdIndex<int>
        //                {
        //                    Id = reader.GetInt32(0),
        //                    Index = reader.GetInt32(0)
        //                });
        //            }
        //        }
        //    }

        //    return result;
        //}

        public void Dispose()
        {
            if (_conn != null)
            {
                _conn.Close();
                _conn.Dispose();
            }
        }
    }
}

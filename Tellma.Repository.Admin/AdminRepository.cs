using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Model.Admin;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Repository.Admin
{
    /// <summary>
    /// A thin and lightweight client for the admin database (Tellma.Database.Admin).
    /// </summary>
    public class AdminRepository : RepositoryBase, IQueryFactory
    {
        private readonly string _connectionString;
        private readonly string _dbName;
        private readonly ILogger<AdminRepository> _logger;
        private readonly IStatementLoader _loader;

        /// <summary>
        /// Implementation of <see cref="RepositoryBase"/>.
        /// </summary>
        protected override ILogger Logger => _logger;

        public AdminRepository(IOptions<AdminRepositoryOptions> options, ILogger<AdminRepository> logger)
        {
            _connectionString = options?.Value?.ConnectionString ?? throw new ArgumentException("The admin connection string was not supplied", nameof(options));
            _dbName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loader = new StatementLoader(_logger);
        }

        #region Queries

        public EntityQuery<T> EntityQuery<T>() where T : Entity => new(ArgumentsFactory);

        public FactQuery<T> FactQuery<T>() where T : Entity => new(ArgumentsFactory);

        public AggregateQuery<T> AggregateQuery<T>() where T : Entity => new(ArgumentsFactory);

        private Task<QueryArguments> ArgumentsFactory(CancellationToken cancellation)
        {
            var queryArgs = new QueryArguments(Sources, _connectionString, _loader);
            return Task.FromResult(queryArgs);
        }

        private static string Sources(Type t)
        {
            return t.Name switch
            {
                nameof(AdminUser) => "[map].[AdminUsers]()",
                nameof(AdminPermission) => "[map].[AdminPermissions]()",
                nameof(SqlDatabase) => "[map].[SqlDatabases]()",
                nameof(SqlServer) => "[map].[SqlServers]()",
                _ => throw new InvalidOperationException($"The requested type {t.Name} is not supported in {nameof(AdminRepository)} queries."),
            };
        }

        public EntityQuery<AdminUser> AdminUsers => EntityQuery<AdminUser>();

        #endregion

        #region Stored Procedures

        public async Task<OnConnectResult> OnConnect(string externalUserId, string userEmail, CancellationToken cancellation)
        {
            OnConnectResult result = null;

            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(OnConnect)}]";

                // Parameters
                cmd.Parameters.Add("@ExternalUserId", externalUserId);
                cmd.Parameters.Add("@UserEmail", userEmail);

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                if (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    // The result
                    result = new OnConnectResult
                    {
                        UserId = reader.Int32(i++),
                        ExternalId = reader.String(i++),
                        Email = reader.String(i++),
                        PermissionsVersion = reader.Guid(i++)?.ToString(),
                        UserSettingsVersion = reader.Guid(i++)?.ToString(),
                    };
                }

                trx.Complete();
            },
            _dbName, nameof(OnConnect), cancellation);

            return result;
        }

        public async Task<IEnumerable<AbstractPermission>> Action_View__Permissions(string action, string view, CancellationToken cancellation)
        {
            List<AbstractPermission> result = null;

            await ExponentialBackoff(async () =>
            {
                result = new List<AbstractPermission>();

                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Action_View__Permissions)}]";

                // Parameters
                cmd.Parameters.Add("@Action", action);
                cmd.Parameters.Add("@View", view);

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    result.Add(new AbstractPermission
                    {
                        View = reader.GetString(i++),
                        Action = reader.GetString(i++),
                        Criteria = reader.String(i++)
                    });
                }

                trx.Complete();
            },
            _dbName, nameof(Action_View__Permissions), cancellation);

            return result;
        }

        public async Task<(Guid, AdminUser, IEnumerable<(string Key, string Value)>)> UserSettings__Load(CancellationToken cancellation)
        {
            Guid version = default;
            AdminUser user = null;
            List<(string, string)> customSettings = null;

            await ExponentialBackoff(async () =>
            {
                user = new AdminUser();
                customSettings = new List<(string, string)>();

                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(UserSettings__Load)}]";

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);

                // Admin user + version
                if (await reader.ReadAsync(cancellation))
                {
                    // TODO: Make these an output parameter
                    int i = 0;

                    user = new AdminUser
                    {
                        Id = reader.GetInt32(i++),
                        Name = reader.String(i++)
                    };

                    version = reader.GetGuid(i++);
                }
                else
                {
                    // Developer mistake
                    throw new InvalidOperationException("No settings for client were found");
                }

                // Custom settings
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    string key = reader.GetString(i++);
                    string val = reader.GetString(i++);

                    customSettings.Add((key, val));
                }

                trx.Complete();
            },
            _dbName, nameof(UserSettings__Load), cancellation);

            return (version, user, customSettings);
        }

        public async Task<AdminSettings> Settings__Load(CancellationToken cancellation)
        {
            AdminSettings result = null;

            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Settings__Load)}]";

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);

                if (await reader.ReadAsync(cancellation))
                {
                    result = new AdminSettings();
                    var props = TypeDescriptor.Get<AdminSettings>().SimpleProperties;
                    foreach (var prop in props)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(result, propValue);
                    }
                }

                trx.Complete();
            },
            _dbName, nameof(Settings__Load), cancellation);

            return result;
        }

        public async Task<(Guid, IEnumerable<AbstractPermission>)> Permissions__Load(CancellationToken cancellation)
        {
            Guid version = default;
            List<AbstractPermission> permissions = null;

            await ExponentialBackoff(async () =>
            {
                permissions = new List<AbstractPermission>();

                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Permissions__Load)}]";

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);

                // Load the version
                if (await reader.ReadAsync(cancellation))
                {
                    // TODO: Make it an output parameter
                    version = reader.GetGuid(0);
                }

                // Load the permissions
                await reader.NextResultAsync(cancellation);

                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    permissions.Add(new AbstractPermission
                    {
                        View = reader.String(i++),
                        Action = reader.String(i++),
                        Criteria = reader.String(i++)
                    });
                }

                trx.Complete();
            },
            _dbName, nameof(Permissions__Load), cancellation);

            return (version, permissions);
        }

        #endregion

        #region Sharding

        /// <summary>
        /// Retrieves the connection information of a given application database or nulls if none are found.
        /// The password key should be used to lookup the password from a secure application configuration provider.
        /// The serverName can be a keyword representing the same server hosting the admin database in which case
        /// the username and password should be retrieved from the admin DB connection string.
        /// </summary>
        /// <param name="databaseId">The application database Id.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The server name, database name, user name and password key of the application database Id, or nulls if none are found</returns>
        public async Task<(string serverName, string dbName, string userName, string passwordKey)> GetDatabaseConnectionInfo(int databaseId, CancellationToken cancellation)
        {
            string serverName = null;
            string dbName = null;
            string userName = null;
            string passwordKey = null;

            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(GetDatabaseConnectionInfo)}]";

                // Parameters
                cmd.Parameters.Add("@DatabaseId", databaseId);

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                if (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    serverName = reader.String(i++);
                    dbName = reader.String(i++);
                    userName = reader.String(i++);
                    passwordKey = reader.String(i++);
                }

                trx.Complete();
            },
            _dbName, nameof(GetDatabaseConnectionInfo), cancellation);

            return (serverName, dbName, userName, passwordKey);
        }

        #endregion

        #region Background Jobs

        /// <summary>
        /// Keeps the current web server instance alive.
        /// <para/>
        /// This call uses a serializable transaction.
        /// </summary>
        /// <param name="instanceId">The Id of the current web server instance.</param>
        /// <param name="keepAliveInSeconds">How long after the most recent heartbeat after which to consider the web server dead, and consider his orphans available for adoption.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public async Task Heartbeat(Guid instanceId, int keepAliveInSeconds, CancellationToken cancellation)
        {
            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Heartbeat)}]";

                // Parameters
                cmd.Parameters.Add("@InstanceId", instanceId);
                cmd.Parameters.Add("@KeepAliveInSeconds", keepAliveInSeconds);

                // Execute
                await conn.OpenAsync(cancellation);
                await cmd.ExecuteNonQueryAsync(cancellation);

                trx.Complete();
            },
            _dbName, nameof(Heartbeat), cancellation);
        }

        /// <summary>
        /// Returns the Ids of any "orphans", ie application databases that are not adopted by any web server instance, so that the current web server can adopt them.
        /// <para/>
        /// This call uses a serializable transaction.
        /// </summary>
        /// <param name="instanceId">The Id of the current web server instance.</param>
        /// <param name="keepAliveInSeconds">How long after the most recent heartbeat after which to consider the web server dead, and consider his orphans available for adoption.</param>
        /// <param name="orphanCount">The maximum number of orphans to adopt.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public async Task<IEnumerable<int>> AdoptOrphans(Guid instanceId, int keepAliveInSeconds, int orphanCount, CancellationToken cancellation)
        {
            List<int> result = null;

            await ExponentialBackoff(async () =>
            {
                result = new List<int>();

                // Connection
                using var trx = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(AdoptOrphans)}]";

                // Parameters
                cmd.Parameters.Add("@InstanceId", instanceId);
                cmd.Parameters.Add("@KeepAliveInSeconds", keepAliveInSeconds);
                cmd.Parameters.Add("@OrphanCount", orphanCount);

                // Execute and Load
                await conn.OpenAsync(cancellation);

                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    result.Add(reader.GetInt32(0));
                }

                trx.Complete();
            },
            _dbName, nameof(AdoptOrphans), cancellation);

            return result;
        }

        #endregion

        #region Directory

        public async Task<(IEnumerable<int> DatabaseIds, bool IsAdmin)> GetAccessibleDatabaseIds(string externalId, string email, CancellationToken cancellation)
        {
            var databaseIds = new List<int>();
            var isAdmin = false;

            await ExponentialBackoff(async () =>
            {
                databaseIds = new List<int>();
                isAdmin = false;

                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(GetAccessibleDatabaseIds)}]";

                // Parameters
                cmd.Parameters.Add("@ExternalId", externalId);
                cmd.Parameters.Add("@Email", email);

                // Execute and Load
                // (1) databaseIds
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    databaseIds.Add(reader.GetInt32(0));
                }

                // (2) isAdmin
                if (await reader.NextResultAsync(cancellation))
                {
                    if (await reader.ReadAsync(cancellation))
                    {
                        isAdmin = reader.GetBoolean(0);
                    }
                }

                trx.Complete();
            },
            _dbName, nameof(GetAccessibleDatabaseIds), cancellation);

            return (databaseIds, isAdmin);
        }

        public async Task DirectoryUsers__SetEmailByExternalId(string externalId, string email)
        {
            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(DirectoryUsers__SetEmailByExternalId)}]";

                // Parameters
                cmd.Parameters.Add("@ExternalId", externalId);
                cmd.Parameters.Add("@Email", email);

                // Execute
                await cmd.ExecuteNonQueryAsync();

                trx.Complete();
            },
            _dbName, nameof(DirectoryUsers__SetEmailByExternalId));
        }

        public async Task DirectoryUsers__SetExternalIdByEmail(string email, string externalId)
        {
            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Parameters
                using var cmd = conn.CreateCommand();
                cmd.Parameters.Add("@Email", email);
                cmd.Parameters.Add("@ExternalId", externalId);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(DirectoryUsers__SetExternalIdByEmail)}]";

                // Execute
                await cmd.ExecuteNonQueryAsync();

                trx.Complete();
            },
            _dbName, nameof(DirectoryUsers__SetExternalIdByEmail));
        }

        public async Task<IEnumerable<string>> DirectoryUsers__Save(IEnumerable<string> newEmails, IEnumerable<string> oldEmails, int databaseId, bool returnEmailsForCreation = false)
        {
            List<string> result = null;
            await ExponentialBackoff(async () =>
            {
                var result = new List<string>();

                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);
                using var cmd = conn.CreateCommand();

                // Parameters
                var newEmailsTable = RepositoryUtilities.DataTable(newEmails.Select(e => new StringListItem { Id = e }));
                var newEmailsTvp = new SqlParameter("@NewEmails", newEmailsTable)
                {
                    TypeName = $"[dbo].[StringList]",
                    SqlDbType = SqlDbType.Structured
                };

                var oldEmailsTable = RepositoryUtilities.DataTable(oldEmails.Select(e => new StringListItem { Id = e }));
                var oldEmailsTvp = new SqlParameter("@OldEmails", oldEmailsTable)
                {
                    TypeName = $"[dbo].[StringList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(newEmailsTvp);
                cmd.Parameters.Add(oldEmailsTvp);
                cmd.Parameters.Add("@DatabaseId", databaseId);
                cmd.Parameters.Add("@ReturnEmailsForCreation", returnEmailsForCreation);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(DirectoryUsers__Save)}]";

                // Execute and load
                if (returnEmailsForCreation)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader.String(0));
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                trx.Complete();
            },
            _dbName, nameof(DirectoryUsers__Save));

            return result;
        }

        #endregion

        #region AdminUsers

        /// <summary>
        /// (1) Adds the given user to AdminUsers if it does not exist.<br/>
        /// (2) Gives that user access to the admin database.<br/>
        /// (3) Gives that user universal permissions if not already.
        /// </summary>
        /// <param name="email">The email of the user to create.</param>
        /// <param name="fullName">The full name of the user to create.</param>
        /// <param name="password">The admin password</param>
        public async Task AdminUsers__CreateAdmin(string email, string fullName)
        {
            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(AdminUsers__CreateAdmin)}]";

                // Parameters
                cmd.Parameters.Add("@Email", email);
                cmd.Parameters.Add("@FullName", fullName);

                // Execute
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                trx.Complete();
            },
            _dbName, nameof(AdminUsers__SaveSettings));
        }

        public async Task AdminUsers__SaveSettings(string key, string value)
        {
            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(AdminUsers__SaveSettings)}]";

                // Parameters
                cmd.Parameters.Add("@Key", key);
                cmd.Parameters.Add("@Value", value);

                // Execute
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                trx.Complete();
            },
            _dbName, nameof(AdminUsers__SaveSettings));
        }

        public async Task AdminUsers__SetEmailByUserId(int userId, string externalEmail)
        {
            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(AdminUsers__SetEmailByUserId)}]";

                // Parameters
                cmd.Parameters.Add("@UserId", userId);
                cmd.Parameters.Add("@ExternalEmail", externalEmail);

                // Execute
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                trx.Complete();
            },
            _dbName, nameof(AdminUsers__SetEmailByUserId));
        }

        public async Task AdminUsers__SetExternalIdByUserId(int userId, string externalId)
        {
            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(AdminUsers__SetExternalIdByUserId)}]";

                // Parameters
                cmd.Parameters.Add("@UserId", userId);
                cmd.Parameters.Add("@ExternalId", externalId);

                // Execute
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                trx.Complete();
            },
            _dbName, nameof(AdminUsers__SetExternalIdByUserId));
        }

        /// <summary>
        /// Preprocesses, validates and saves the list of <see cref="AdminUser"/> entities in the database
        /// and optionally returns their Ids with the input order preserved.
        /// </summary>
        /// <param name="entities">The entities to preprocess, validate and save.</param>
        /// <param name="returnIds">Whether or not to return the entity Ids</param>
        /// <param name="ctx">Session context data.</param>
        /// <returns>A <see cref="SaveResult"/> object containing the validation errors if any and the
        /// Ids of the saved entities if requested and the entities returned no validation errors</returns>
        public async Task<SaveResult> AdminUsers__Save(List<AdminUserForSave> entities, bool returnIds, int userId)
        {
            SaveResult result = null;
            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(AdminUsers__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(AdminUser)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable permissionsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Permissions);
                var permissionsTvp = new SqlParameter("@Permissions", permissionsTable)
                {
                    TypeName = $"[dbo].[{nameof(AdminPermission)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(permissionsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds);

                trx.Complete();
            },
            _dbName, nameof(AdminUsers__Save));

            return result;
        }

        /// <summary>
        /// Deletes all entities with Ids present in the supplied list after validating that they can be deleted.
        /// </summary>
        /// <param name="ids">The ids to delete.</param>
        /// <param name="ctx">Session context data.</param>
        public async Task<DeleteResult> AdminUsers__Delete(IEnumerable<int> ids, int userId)
        {
            DeleteResult result = null;

            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(AdminUsers__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();

                    trx.Complete();
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            _dbName, nameof(AdminUsers__Delete));

            return result;
        }

        /// <summary>
        /// Activated or deactivates all entities with Ids present in the supplied list after validating that they can be activated or deactivated.
        /// </summary>
        /// <param name="ids">The ids to activate or deactivate.</param>
        /// <param name="isActive">Whether to activate the entities or deactivate them</param>
        /// <param name="ctx">Session context data.</param>
        public async Task<OperationResult> AdminUsers__Activate(List<int> ids, bool isActive, int userId)
        {
            OperationResult result = null;

            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(_connectionString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(AdminUsers__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }));
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@UserId", userId);


                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult();

                trx.Complete();
            },
            _dbName, nameof(AdminUsers__Activate));

            return result;
        }

        #endregion
    }
}


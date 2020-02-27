﻿using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.ClientInfo;
using Tellma.Services.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Tellma.Data
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", 
        Justification = "To maintain the SESSION_CONTEXT we keep a hold of the SqlConnection object for the lifetime of the repository")]
    public class AdminRepository : IRepository, IDisposable
    {
        private readonly IExternalUserAccessor _externalUserAccessor;
        private readonly IClientInfoAccessor _clientInfoAccessor;
        private readonly IStringLocalizer _localizer;
        private readonly string _connectionString;

        private SqlConnection _conn;
        private Transaction _transactionOverride;
        private AdminUserInfo _userInfo;

        #region Lifecycle

        public AdminRepository(IOptions<AdminRepositoryOptions> config, IExternalUserAccessor externalUserAccessor,
            IClientInfoAccessor clientInfoAccessor, IStringLocalizer<Strings> localizer)
        {
            _connectionString = config?.Value?.ConnectionString ?? throw new ArgumentException("The admin connection string was not supplied", nameof(config));
            _externalUserAccessor = externalUserAccessor;
            _clientInfoAccessor = clientInfoAccessor;
            _localizer = localizer;
        }

        public void Dispose()
        {
            if (_conn != null)
            {
                _conn.Close();
                _conn.Dispose();
            }
        }

        #endregion

        #region Connection Management

        /// <summary>
        /// Initializes the connection if it is not already initialized
        /// </summary>
        /// <returns>The connection string that was initialized</returns>
        private async Task<SqlConnection> GetConnectionAsync()
        {
            if (_conn == null)
            {
                _conn = new SqlConnection(_connectionString);
                await _conn.OpenAsync();
            }

            if (_userInfo == null)
            {
                // Always call OnConnect SP as soon as you create the connection
                var externalUserId = _externalUserAccessor.GetUserId();
                var externalEmail = _externalUserAccessor.GetUserEmail();
                var culture = CultureInfo.CurrentUICulture.Name;
                var neutralCulture = CultureInfo.CurrentUICulture.IsNeutralCulture ? CultureInfo.CurrentUICulture.Name : CultureInfo.CurrentUICulture.Parent.Name;

                _userInfo = await OnConnect(externalUserId, externalEmail, culture, neutralCulture);
            }

            // Since we opened the connection once, we need to explicitly enlist it in any ambient transaction
            // every time it is requested, otherwise commands will be executed outside the boundaries of the transaction
            _conn.EnlistInTransaction(transactionOverride: _transactionOverride);
            return _conn;
        }

        /// <summary>
        /// Initializes the connection if it is not already initialized, this version does
        /// not invoke <see cref="OnConnect(string, string, string, string)"/>, it is used
        /// to retrieve metadata from the admin database such as the accessible database Id
        /// </summary>
        /// <returns>The connection string that was initialized</returns>
        private async Task<SqlConnection> GetDirectoryConnectionAsync()
        {
            if (_conn == null)
            {
                _conn = new SqlConnection(_connectionString);
                await _conn.OpenAsync();
            }

            // Since we opened the connection once, we need to explicitly enlist it in any ambient transaction
            // every time it is requested, otherwise commands will be executed outside the boundaries of the transaction
            _conn.EnlistInTransaction(transactionOverride: _transactionOverride);
            return _conn;
        }

        /// <summary>
        /// Loads a <see cref="AdminUserInfo"/> object from the database, this occurs once per <see cref="ApplicationRepository"/> 
        /// instance, subsequent calls are satisfied from a scoped cache
        /// </summary>
        public async Task<AdminUserInfo> GetAdminUserInfoAsync()
        {
            await GetConnectionAsync(); // This automatically initializes the user info
            return _userInfo;
        }

        /// <summary>
        /// Enlists the repository's connection in the provided transaction such that all subsequent commands particupate in it, regardless of the ambient transaction
        /// </summary>
        /// <param name="transaction">The transaction to enlist the connection in</param>
        public void EnlistTransaction(Transaction transaction)
        {
            _transactionOverride = transaction;
        }

        #endregion

        #region Queries

        public Query<AdminUser> AdminUsers => Query<AdminUser>();

        public Query<T> Query<T>() where T : Entity
        {
            return new Query<T>(GetFactory());
        }

        public AggregateQuery<T> AggregateQuery<T>() where T : Entity
        {
            return new AggregateQuery<T>(GetFactory());
        }

        private QueryArgumentsFactory GetFactory()
        {
            async Task<QueryArguments> Factory()
            {
                var conn = await GetConnectionAsync();
                var userInfo = await GetAdminUserInfoAsync();
                var userId = userInfo.UserId ?? 0;
                var userToday = _clientInfoAccessor.GetInfo().Today;

                return new QueryArguments(conn, Sources, userId, userToday, _localizer);
            }

            return Factory;
        }

        private static string Sources(Type t)
        {
            return t.Name switch
            {
                nameof(AdminUser) => "[map].[AdminUsers]()",
                nameof(AdminPermission) => "[map].[AdminPermissions]()",
                nameof(SqlDatabase) => "[map].[SqlDatabases]()",
                nameof(SqlServer) => "[map].[SqlServers]()",
                _ => throw new InvalidOperationException($"The requested type {t.Name} is not supported in {nameof(AdminRepository)} queries"),
            };
        }

        #endregion

        #region Stored Procedures

        private async Task<AdminUserInfo> OnConnect(string externalUserId, string userEmail, string culture, string neutralCulture)
        {
            AdminUserInfo result = null;

            using (SqlCommand cmd = _conn.CreateCommand()) // Use the private field _conn to avoid infinite recursion
            {
                // Parameters
                cmd.Parameters.Add("@ExternalUserId", externalUserId);
                cmd.Parameters.Add("@UserEmail", userEmail);
                cmd.Parameters.Add("@Culture", culture);
                cmd.Parameters.Add("@NeutralCulture", neutralCulture);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(OnConnect)}]";

                // Execute and Load
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int i = 0;

                    // The user Info
                    result = new AdminUserInfo
                    {
                        UserId = reader.Int32(i++),
                        ExternalId = reader.String(i++),
                        Email = reader.String(i++),
                        PermissionsVersion = reader.Guid(i++)?.ToString(),
                        UserSettingsVersion = reader.Guid(i++)?.ToString(),
                    };
                }
                else
                {
                    throw new InvalidOperationException($"[dal].[OnConnect] did not return any data from Admin Database, ExternalUserId: {externalUserId}, UserEmail: {userEmail}");
                }
            }

            return result;
        }

        public async Task<IEnumerable<AbstractPermission>> Action_View__Permissions(string action, string view)
        {
            var result = new List<AbstractPermission>();

            var conn = await GetConnectionAsync();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.Add("@Action", action);
                cmd.Parameters.Add("@View", view);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Action_View__Permissions)}]";

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int i = 0;
                    result.Add(new AbstractPermission
                    {
                        View = reader.GetString(i++),
                        Action = reader.GetString(i++),
                        Criteria = reader.String(i++)
                    });
                }
            }

            return result;
        }

        public async Task<(Guid, AdminUser, IEnumerable<(string Key, string Value)>)> UserSettings__Load()
        {
            Guid version;
            var user = new AdminUser();
            var customSettings = new List<(string, string)>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(UserSettings__Load)}]";

                // Execute
                using var reader = await cmd.ExecuteReaderAsync();

                // User Settings
                if (await reader.ReadAsync())
                {
                    int i = 0;

                    user.Id = reader.GetInt32(i++);
                    user.Name = reader.String(i++);

                    version = reader.GetGuid(i++);
                }
                else
                {
                    // Developer mistake
                    throw new InvalidOperationException("No settings for client were found");
                }

                // Custom settings
                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    string key = reader.GetString(0);
                    string val = reader.GetString(1);

                    customSettings.Add((key, val));
                }
            }

            return (version, user, customSettings);
        }

        public async Task<AdminSettings> Settings__Load()
        {
            // Returns 
            // (1) the settings with the functional currency expanded

            AdminSettings settings = new AdminSettings();

            var conn = await GetConnectionAsync();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Settings__Load)}]";

                // Execute
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var props = typeof(AdminSettings).GetMappedProperties();
                    foreach (var prop in props)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(settings, propValue);
                    }
                }
                else
                {
                    // Programmer mistake
                    throw new Exception($"AdminSettings was not returned from SP {nameof(Settings__Load)}");
                }
            }

            return settings;
        }

        public async Task<(Guid, IEnumerable<AbstractPermission>)> Permissions__Load()
        {
            Guid version;
            var permissions = new List<AbstractPermission>();

            var conn = await GetConnectionAsync();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Permissions__Load)}]";

                // Execute
                using var reader = await cmd.ExecuteReaderAsync();
                // Load the version
                if (await reader.ReadAsync())
                {
                    version = reader.GetGuid(0);
                }
                else
                {
                    version = Guid.Empty;
                }

                // Load the permissions
                await reader.NextResultAsync();

                while (await reader.ReadAsync())
                {
                    int i = 0;
                    permissions.Add(new AbstractPermission
                    {
                        View = reader.String(i++),
                        Action = reader.String(i++),
                        Criteria = reader.String(i++)
                    });
                }
            }

            return (version, permissions);
        }

        #endregion

        #region Directory Stuff

        public async Task<DatabaseConnectionInfo> GetDatabaseConnectionInfo(int databaseId)
        {
            DatabaseConnectionInfo result = null;

            var conn = await GetDirectoryConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.Add("@DatabaseId", databaseId);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(GetDatabaseConnectionInfo)}]";

                // Execute and Load
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int i = 0;

                    // The user Info
                    result = new DatabaseConnectionInfo
                    {
                        ServerName = reader.String(i++),
                        DatabaseName = reader.String(i++),
                        UserName = reader.String(i++),
                        PasswordKey = reader.String(i++),
                    };
                }
            }

            return result;
        }

        public async Task<(IEnumerable<int> DatabaseIds, bool IsAdmin)> GetAccessibleDatabaseIds(string externalId, string email)
        {
            var databaseIds = new List<int>();
            var isAdmin = false;

            var conn = await GetDirectoryConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            cmd.Parameters.Add("ExternalId", externalId);
            cmd.Parameters.Add("Email", email);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(GetAccessibleDatabaseIds)}]";

            // Execute and Load
            using var reader = await cmd.ExecuteReaderAsync();

            // First the DB Ids
            while (await reader.ReadAsync())
            {
                databaseIds.Add(reader.GetInt32(0));
            }

            // Then Is Admin
            await reader.NextResultAsync();
            if (await reader.ReadAsync())
            {
                isAdmin = reader.GetBoolean(0);
            }

            return (databaseIds, isAdmin);
        }

        public async Task DirectoryUsers__SetEmailByExternalId(string externalId, string email)
        {
            var conn = await GetDirectoryConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            cmd.Parameters.Add("ExternalId", externalId);
            cmd.Parameters.Add("Email", email);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(DirectoryUsers__SetEmailByExternalId)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DirectoryUsers__SetExternalIdByEmail(string email, string externalId)
        {
            var conn = await GetDirectoryConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            cmd.Parameters.Add("Email", email);
            cmd.Parameters.Add("ExternalId", externalId);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(DirectoryUsers__SetExternalIdByEmail)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<string>> DirectoryUsers__Save(IEnumerable<string> newEmails, IEnumerable<string> oldEmails, int databaseId, bool returnEmailsForCreation = false)
        {
            var result = new List<string>();

            var conn = await GetDirectoryConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
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
            }

            return result;
        }

        #endregion

        #region AdminUsers

        public async Task AdminUsers__SaveSettings(string key, string value)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            cmd.Parameters.Add("Key", key);
            cmd.Parameters.Add("Value", value);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(AdminUsers__SaveSettings)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AdminUsers__CreateAdmin(string email, string fullName, string password, string adminServerDescription = null)
        {
            // 1 - Adds the given user to AdminUsers (if it does not exist)
            // 2 - Gives that user access to the admin database
            // 3 - Gives that user universal permissions (if not already)

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            cmd.Parameters.Add("Email", email);
            cmd.Parameters.Add("FullName", fullName);
            cmd.Parameters.Add("Password", password);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(AdminUsers__CreateAdmin)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AdminUsers__SetEmailByUserId(int userId, string externalEmail)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            cmd.Parameters.Add("UserId", userId);
            cmd.Parameters.Add("ExternalEmail", externalEmail);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(AdminUsers__SetEmailByUserId)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AdminUsers__SetExternalIdByUserId(int userId, string externalId)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            cmd.Parameters.Add("UserId", userId);
            cmd.Parameters.Add("ExternalId", externalId);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(AdminUsers__SetExternalIdByUserId)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> AdminUsers_Validate__Save(List<AdminUserForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

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
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(AdminUsers_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> AdminUsers__Save(List<AdminUserForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
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

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(AdminUsers__Save)}]";

                // Execute
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int i = 0;
                    result.Add(new IndexedId
                    {
                        Index = reader.GetInt32(i++),
                        Id = reader.GetInt32(i++)
                    });
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task<IEnumerable<ValidationError>> AdminUsers_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(AdminUsers_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task AdminUsers__Delete(IEnumerable<int> ids)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }));
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(AdminUsers__Delete)}]";

            // Execute
            try
            {
                // Execute
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        public async Task AdminUsers__Activate(List<int> ids, bool isActive)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }));
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(AdminUsers__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        #endregion
    }
}
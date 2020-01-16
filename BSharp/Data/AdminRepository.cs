using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.ClientInfo;
using BSharp.Services.Identity;
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

namespace BSharp.Data
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "To maintain the SESSION_CONTEXT we keep a hold of the SqlConnection object for the lifetime of the repository")]
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
                var sources = GetSources();
                var userInfo = await GetAdminUserInfoAsync();
                var userId = userInfo.UserId ?? 0;
                var userToday = _clientInfoAccessor.GetInfo().Today;

                return new QueryArguments(conn, sources, userId, userToday, _localizer);
            }

            return Factory;
        }

        private static Func<Type, SqlSource> GetSources()
        {
            return (t) =>
            {
                return t.Name switch
                {
                    nameof(AdminUser) => new SqlSource("[dbo].[GlobalUsers]"),
                    nameof(SqlDatabase) => new SqlSource("[dbo].[SqlDatabases]"),
                    nameof(SqlServer) => new SqlSource("[dbo].[SqlServers]"),
                    nameof(GlobalUserMembership) => new SqlSource("[dbo].[GlobalUserMemberships]"),
                    _ => throw new InvalidOperationException($"The requested type {t.Name} is not supported in {nameof(AdminRepository)} queries"),
                };
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
                    };
                }
                else
                {
                    throw new InvalidOperationException($"[dal].[OnConnect] did not return any data from Admin Database, ExternalUserId: {externalUserId}, UserEmail: {userEmail}");
                }
            }

            return result;
        }

        public async Task<DatabaseConnectionInfo> GetDatabaseConnectionInfo(int databaseId)
        {
            DatabaseConnectionInfo result = null;

            var conn = await GetConnectionAsync();
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

        public async Task<IEnumerable<int>> GetAccessibleDatabaseIds()
        {
            var result = new List<int>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(GetAccessibleDatabaseIds)}]";

                // Execute and Load
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetInt32(0));
                }
            }

            return result;
        }

        #endregion

        #region GlobalUsers

        public async Task GlobalUsers__SetExternalIdByUserId(int userId, string externalId)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            cmd.Parameters.Add("UserId", userId);
            cmd.Parameters.Add("ExternalId", externalId);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(GlobalUsers__SetExternalIdByUserId)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task GlobalUsers__SetEmailByUserId(int userId, string externalEmail)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            cmd.Parameters.Add("UserId", userId);
            cmd.Parameters.Add("ExternalEmail", externalEmail);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(GlobalUsers__SetEmailByUserId)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task GlobalUsers__SetExternalIdByEmail(string email, string externalId)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            cmd.Parameters.Add("Email", email);
            cmd.Parameters.Add("ExternalId", externalId);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(GlobalUsers__SetExternalIdByEmail)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<string>> GlobalUsers__Save(IEnumerable<string> newEmails, IEnumerable<string> oldEmails, int databaseId, bool returnEmailsForCreation = false)
        {
            var result = new List<string>();

            var conn = await GetConnectionAsync();
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
                cmd.CommandText = $"[dal].[{nameof(GlobalUsers__Save)}]";

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

        #endregion
    }
}

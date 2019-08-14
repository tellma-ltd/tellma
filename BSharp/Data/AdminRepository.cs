using BSharp.Data.Queries;
using BSharp.EntityModel;
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
        private GlobalUserInfo _userInfo;

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
                _conn.Open();

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
        /// Loads a <see cref="GlobalUserInfo"/> object from the database, this occurs once per <see cref="ApplicationRepository"/> 
        /// instance, subsequent calls are satisfied from a scoped cache
        /// </summary>
        public async Task<GlobalUserInfo> GetGlobalUserInfoAsync()
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
                var userInfo = await GetGlobalUserInfoAsync();
                var userId = userInfo.UserId ?? 0;
                var userTimeZone = _clientInfoAccessor.GetInfo().TimeZone;

                return new QueryArguments(conn, sources, userId, userTimeZone, _localizer);
            }

            return Factory;
        }

        private static Func<Type, SqlSource> GetSources()
        {
            return (t) =>
            {
                switch (t.Name)
                {
                    case nameof(GlobalUser):
                        return new SqlSource("[dbo].[GlobalUsers]");

                    case nameof(SqlDatabase):
                        return new SqlSource("[dbo].[SqlDatabases]");

                    case nameof(SqlServer):
                        return new SqlSource("[dbo].[SqlServers]");

                    case nameof(GlobalUserMembership):
                        return new SqlSource("[dbo].[GlobalUserMemberships]");

                    default:
                        throw new InvalidOperationException($"The requested type {t.Name} is not supported in {nameof(AdminRepository)} queries");
                }
            };
        }

        #endregion

        #region Stored Procedures

        private async Task<GlobalUserInfo> OnConnect(string externalUserId, string userEmail, string culture, string neutralCulture)
        {
            GlobalUserInfo result = null;

            using (SqlCommand cmd = _conn.CreateCommand()) // Use the private field _conn to avoid infinite recursion
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
                        result = new GlobalUserInfo
                        {
                            UserId = reader.IsDBNull(i) ? (int?)null : reader.GetInt32(i++),
                            ExternalId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            Email = reader.IsDBNull(i) ? null : reader.GetString(i++),
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException($"[dal].[OnConnect] did not return any data from Admin Database, ExternalUserId: {externalUserId}, UserEmail: {userEmail}");
                    }
                }
            }

            return result;
        }

        public Task SetUserExternalIdByUserIdAsync(int userId, string externalId)
        {
            throw new NotImplementedException();
        }

        public Task SetUserEmailByUserIdAsync(int userId, string externalEmail)
        {
            throw new NotImplementedException();
        }

        public Task SetUserExternalIdByEmailAsync(string email, string externalId)
        {
            // Finds the user with the given email and sets its externalId as specified
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string>> GlobalUsers__Save(IEnumerable<string> newEmails, IEnumerable<string> oldEmails, int databaseId, bool returnEmailsForCreation = false)
        {
            var result = new List<string>();

            var conn = await GetConnectionAsync();
            using(var cmd = conn.CreateCommand())
            {
                // Parameters
                var newEmailsTable = RepositoryUtilities.DataTable(newEmails.Select(e => new StringListItem { Id = e }));
                var newEmailsTvp = new SqlParameter("@NewEmails", newEmailsTable)
                {
                    TypeName = $"dbo.StringList",
                    SqlDbType = SqlDbType.Structured
                };
                
                var oldEmailsTable = RepositoryUtilities.DataTable(oldEmails.Select(e => new StringListItem { Id = e }));
                var oldEmailsTvp = new SqlParameter("@OldEmails", oldEmailsTable)
                {
                    TypeName = $"dbo.StringList",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.AddWithValue("@DatabaseId", databaseId);
                cmd.Parameters.AddWithValue("@ReturnEmailsForCreation", returnEmailsForCreation);

                // Command
                cmd.CommandText = $@"EXEC [dbo].[{nameof(GlobalUsers__Save)}] 
@NewEmails = @NewEmails, 
@OldEmails = @OldEmails, 
@DatabaseId = @DatabaseId, 
@ReturnEmailsForCreation = @ReturnEmailsForCreation";

                // Execute and load
                if(returnEmailsForCreation)
                {
                    using(var reader = await cmd.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            result.Add(reader.GetString(0));
                        }
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
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
                cmd.Parameters.AddWithValue("@DatabaseId", databaseId);

                // Command
                cmd.CommandText = $@"EXEC [dal].[{nameof(GetDatabaseConnectionInfo)}] @DatabaseId = @DatabaseId";

                // Execute and Load
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int i = 0;

                        // The user Info
                        result = new DatabaseConnectionInfo
                        {
                            ServerName = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            DatabaseName = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            UserName = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            PasswordKey = reader.IsDBNull(i) ? null : reader.GetString(i++),
                        };
                    }
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
                cmd.CommandText = $"EXEC [dal].[{nameof(GetAccessibleDatabaseIds)}]";

                // Execute and Load
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader.GetInt32(0));
                    }
                }
            }

            return result;
        }

        #endregion
    }
}

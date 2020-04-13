using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.ClientInfo;
using Tellma.Services.EmbeddedIdentityServer;
using Tellma.Services.Identity;

namespace Tellma.Data
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", 
        Justification = "To maintain the SESSION_CONTEXT we keep a hold of the SqlConnection object for the lifetime of the repository")]
    public class IdentityRepository : IRepository, IDisposable
    {
        private readonly IExternalUserAccessor _externalUserAccessor;
        private readonly IClientInfoAccessor _clientInfoAccessor;
        private readonly IStringLocalizer _localizer;
        private readonly string _connectionString;

        private SqlConnection _conn;
        private Transaction _transactionOverride;

        #region Lifecycle

        public IdentityRepository(IOptions<EmbeddedIdentityServerOptions> config, IExternalUserAccessor externalUserAccessor,
            IClientInfoAccessor clientInfoAccessor, IStringLocalizer<Strings> localizer)
        {
            _connectionString = config?.Value?.ConnectionString ?? throw new ArgumentException("The identity connection string was not supplied", nameof(config));
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
        private async Task<SqlConnection> GetConnectionAsync(CancellationToken cancellation = default)
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
            return new Query<T>(Factory);
        }

        public AggregateQuery<T> AggregateQuery<T>() where T : Entity
        {
            return new AggregateQuery<T>(Factory);
        }

        private async Task<QueryArguments> Factory(CancellationToken cancellation)
        {
            var conn = await GetConnectionAsync(cancellation);
            var userToday = _clientInfoAccessor.GetInfo().Today;

            return new QueryArguments(conn, Sources, 0, userToday, _localizer);
        }

        private static string Sources(Type t)
        {
            return t.Name switch
            {
                nameof(IdentityServerUser) => "[map].[IdentityServerUsers]()",
                _ => throw new InvalidOperationException($"The requested type {t.Name} is not supported in {nameof(IdentityRepository)} queries"),
            };
        }

        #endregion
    }
}

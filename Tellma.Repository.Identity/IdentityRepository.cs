using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Common;
using Tellma.Model.Identity;
using Tellma.Repository.Common;

namespace Tellma.Repository.Identity
{
    /// <summary>
    /// A thin and lightweight client for the admin database (Tellma.Database.Admin).
    /// </summary>
    public class IdentityRepository : RepositoryBase, IQueryFactory
    {
        #region Lifecycle

        private readonly string _connectionString;
        private readonly string _dbName;
        private readonly ILogger<IdentityRepository> _logger;
        private readonly IStatementLoader _loader;

        /// <summary>
        /// Implementation of <see cref="RepositoryBase"/>.
        /// </summary>
        protected override ILogger Logger => _logger;

        public IdentityRepository(IOptions<IdentityRepositoryOptions> options, ILogger<IdentityRepository> logger)
        {
            _connectionString = options?.Value?.ConnectionString ?? throw new ArgumentException("The admin connection string was not supplied", nameof(options));
            _dbName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loader = new StatementLoader(_logger);
        }

        #endregion

        #region Queries

        public EntityQuery<T> EntityQuery<T>() where T : Entity => new(ArgumentsFactory);

        public FactQuery<T> FactQuery<T>() where T : Entity => new(ArgumentsFactory);

        public AggregateQuery<T> AggregateQuery<T>() where T : Entity => new(ArgumentsFactory);

        private Task<QueryArguments> ArgumentsFactory(CancellationToken cancellation)
        {
            var queryArgs = new QueryArguments(Sources, _connectionString, _loader);
            return Task.FromResult(queryArgs);
        }

        private static string Sources(Type t) => t.Name switch
        {
            nameof(IdentityServerUser) => "[map].[IdentityServerUsers]()",
            _ => throw new InvalidOperationException($"The requested type {t.Name} is not supported in {nameof(IdentityRepository)} queries.")
        };

        public EntityQuery<IdentityServerUser> IdentityServerUsers => EntityQuery<IdentityServerUser>();

        #endregion
    }
}


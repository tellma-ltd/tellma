using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Utilities.Sharding;

namespace Tellma.Repository.Application.Tests
{
    public class ApplicationRepositoryFixture
    {
        public ApplicationRepository Repo { get; }

        public ApplicationRepositoryFixture()
        {
            // Build the configuration root based on project user secrets
            IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets(GetType().Assembly)
                .Build();

            // Retrieve the admin connection string from project user secrets
            var dbName = config.GetValue<string>("DatabaseName");
            var serverName = config.GetValue<string>("ServerName");

            // Default values
            if (string.IsNullOrWhiteSpace(dbName))
            {
                dbName = "Tellma.Tests.101";
            }

            if (string.IsNullOrWhiteSpace(serverName))
            {
                serverName = ".";
            }

            // Create the connection string
            var connBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = dbName,
                IntegratedSecurity = true, // Windows Auth
                MultipleActiveResultSets = true,
            };

            var connString = connBuilder.ConnectionString;

            // Prepare the mock dependencies
            var logger = new NullLogger<ApplicationRepository>();
            var shardResolver = new MockShardResolver(connString);

            // Populate the read-only Fixture fields
            Repo = new ApplicationRepository(MockShardResolver.TenantId, shardResolver, logger);
        }

        #region Mocks

        private class MockShardResolver : IShardResolver
        {
            public const int TenantId = 101;
            private readonly string _connString;

            public MockShardResolver(string connString) => _connString = connString;

            public Task<string> GetConnectionString(int databaseId, CancellationToken cancellation)
                => Task.FromResult(databaseId == TenantId ? _connString : default);
        }

        #endregion
    }
}

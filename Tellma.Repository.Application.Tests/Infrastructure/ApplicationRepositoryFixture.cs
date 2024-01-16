using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Utilities.Sharding;

namespace Tellma.Repository.Application.Tests
{
    public class ApplicationRepositoryFixture
    {
        private const string TELLMA_DATABASE_SECTION_NAME = "TellmaDatabase";

        public ApplicationRepository Repo { get; }

        public ApplicationRepositoryFixture()
        {
            // Build the configuration root based on project user secrets
            var config = new ConfigurationBuilder()
                .AddUserSecrets(GetType().Assembly)
                .Build();

            // Retrieve the database connection string from project user secrets
            var options = config.GetSection(TELLMA_DATABASE_SECTION_NAME)
                ?.Get<TellmaDatabaseOptions>()
                ?? throw new InvalidOperationException($"Running the tests requires a '{TELLMA_DATABASE_SECTION_NAME}' section in the Test project user secrets");

            // Prepare the mock dependencies
            var logger = new NullLogger<ApplicationRepository>();
            var shardResolver = new MockShardResolver(options.ConnectionString);

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

    public class TellmaDatabaseOptions
    {
        public string ConnectionString { get; set; }
    }
}

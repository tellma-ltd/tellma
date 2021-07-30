using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Application;
using Tellma.Repository.Common;
using Tellma.Utilities.Sharding;
using Xunit;

namespace Tellma.Repository.Application.Tests
{
    public class ApplicationRepositoryFixture : IAsyncLifetime
    {
        public ApplicationRepository Repo { get; }

        public int UserId { get; private set; }

        public ApplicationRepositoryFixture()
        {
            // Build the configuration root based on project user secrets
            IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets(GetType().Assembly)
                .Build();

            const string configKey = "ApplicationConnection";

            // Retrieve the admin connection string from project user secrets
            var connectionString = config
                .GetValue<string>(configKey);

            // Validation
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // To run the tests, the developer should deploy the admin DB locally and specify the
                // connection connection string to the local admin dB in the test project's user secrets
                throw new InvalidOperationException($"Must specify the value '{configKey}' in the project's user secrets.");
            }

            // Prepare the AdminRepository dependencies
            var logger = new NullLogger<ApplicationRepository>();
            var shardResolver = new MockShardResolver(connectionString);

            // Populate the read-only Fixture fields
            Repo = new ApplicationRepository(MockShardResolver.TenantId, shardResolver, logger);
        }

        public async Task InitializeAsync()
        {
            // Get the userId
            var user = await Repo.EntityQuery<User>().FirstOrDefaultAsync(new QueryContext(0));
            if (user == null)
            {
                throw new InvalidOperationException("Not users in the database.");
            }
            UserId = user.Id;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
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

    [CollectionDefinition(nameof(ApplicationRepositoryCollection))]
    public class ApplicationRepositoryCollection : ICollectionFixture<ApplicationRepositoryFixture>
    {
        // Empty class just to define the collection
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Tellma.Repository.Admin.Tests
{
    public class AdminRepositoryFixture : IAsyncLifetime
    {
        public AdminRepository Repo { get; }

        public int UserId { get; }

        public AdminRepositoryFixture()
        {
            // Build the configuration root based on project user secrets
            IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets(GetType().Assembly)
                .Build();

            // Retrieve the admin connection string from project user secrets
            var connectionString = config
                .GetValue<string>("AdminConnection");

            // Validation
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // To run the tests, the developer should deploy the admin DB locally and specify the
                // connection connection string to the local admin dB in the test project's user secrets
                throw new InvalidOperationException("Must specify the value 'AdminConnection' in the project's user secrets.");
            }

            // Prepare the AdminRepository dependencies
            var logger = new NullLogger<AdminRepository>();
            var options = new AdminRepositoryOptions { ConnectionString = connectionString };

            // Populate the read-only Fixture fields
            Repo = new AdminRepository(Options.Create(options), logger);
            UserId = 3; // TODO: Retrieve with OnConnect
        }

        public async Task InitializeAsync()
        {
            // Here we setup the database for all the unit tests
            await Repo.AdminUsers__CreateAdmin("admin@tellma.com", "Administrator");
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }

    [CollectionDefinition(nameof(AdminRepositoryCollection))]
    public class AdminRepositoryCollection : ICollectionFixture<AdminRepositoryFixture>
    {
        // Empty class just to define the collection
    }
}

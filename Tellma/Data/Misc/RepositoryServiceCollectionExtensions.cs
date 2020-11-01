using Tellma.Data;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RepositoryServiceCollectionExtensions
    {
        /// <summary>
        /// Register the <see cref="AdminRepository"/> in the DI container
        /// </summary>
        /// <param name="connectionString">The connection string of the admin database</param>
        public static IServiceCollection AddAdminRepository(this IServiceCollection services, string connectionString)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("The admin connection string must be specified in a configuration provider", nameof(connectionString));
            }

            services.AddOptions();

            // Now the Admin repository can resolve this options class and retrieve the connection string
            services.Configure<AdminRepositoryOptions>(opt =>
            {
                opt.ConnectionString = connectionString;
            });

            return services.AddScoped<AdminRepository>()
                .AddScoped<IdentityRepository>();
        }

        /// <summary>
        /// Register the <see cref="ApplicationRepository"/> in the DI container, this repository
        /// provides access to tenant specific database
        /// </summary>
        public static IServiceCollection AddApplicationRepository(this IServiceCollection services)
        {
            return services.AddScoped<ApplicationRepository>()
                .AddSingleton<ApplicationRepositoryLite>();
        }
    }
}

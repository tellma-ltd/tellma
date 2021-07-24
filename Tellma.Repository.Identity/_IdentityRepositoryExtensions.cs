using System;
using Tellma.Repository.Identity;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class _IdentityRepositoryExtensions
    {
        /// <summary>
        /// Registers the <see cref="IdentityRepository"/> providing access to the identity database.
        /// </summary>
        public static IServiceCollection AddIdentityRepository(this IServiceCollection services, string connString)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new ArgumentException($"'{nameof(connString)}' cannot be null or whitespace.", nameof(connString));
            }

            // Allows the Identity repository to resolve this options class and retrieve the connection string
            services.Configure<IdentityRepositoryOptions>(opt =>
            {
                opt.ConnectionString = connString;
            });

            // Add services
            return services.AddSingleton<IdentityRepository>();
        }
    }
}

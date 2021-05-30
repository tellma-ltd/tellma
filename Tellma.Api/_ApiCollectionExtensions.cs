using Microsoft.Extensions.Configuration;
using System;
using Tellma.Api;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TellmaApiServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all the services of the Tellma API.
        /// </summary>
        public static IServiceCollection AddTellmaApi(this IServiceCollection services, IConfiguration config = null)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is not null)
            {

            }

            // Add repositories
            var connString = config.GetConnectionString("AdminConnection");
            services.AddAdminRepository(connString);

            services = services.AddScoped<MetadataProvider>(); // This was scoped before

            // Add business services
            return services.AddScoped<AdminUsersService>();
        }
    }
}

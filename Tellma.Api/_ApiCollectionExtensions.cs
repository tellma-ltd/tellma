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

            // Add admin repository
            var adminConnString = config.GetConnectionString("AdminConnection");
            services.AddAdminRepository(adminConnString);

            // Add application repository
            services.AddApplicationRepository();

            // Add infrastructure services
            services = services
                .AddMetadata()
                .AddImportExport()
                .AddMarkupTemplates(); 

            // Add business services
            return services.AddScoped<AdminUsersService>();
        }
    }
}

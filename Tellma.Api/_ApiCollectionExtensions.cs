using Microsoft.Extensions.Configuration;
using System;
using Tellma.Api;
using Tellma.Api.Behaviors;

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

            // (1) Add admin and application repository
            var adminConnString = config.GetConnectionString("AdminConnection");
            services.AddAdminRepository(adminConnString);
            services.AddApplicationRepository();

            // (2) Add infrastructure services
            services.AddTellmaApiBase(); // Add IApiClient of both templating and import/export

            // (3) Add cache

            // (4) Add behaviors
            services
                .AddScoped<ApplicationVersions>()
                .AddScoped<AdminServiceBehavior>()
                .AddScoped<ApplicationServiceBehavior>()
                .AddScoped<ApplicationFactServiceBehavior>();

            // (5) Add base Dependencies
            services
                .AddScoped<SettingsServiceDependencies>();

            // (5) Add API services
            // services.AddScoped<AdminUsersService>();

            return services;
        }
    }
}
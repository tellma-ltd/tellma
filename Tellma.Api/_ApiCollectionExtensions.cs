using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Tellma.Api;
using Tellma.Api.Behaviors;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiCollectionExtensions
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
            services.AddTellmaApiBase(); // TODO: Add IApiClient of both templating and import/export

            // (3) Add cache

            // (4) Add behaviors
            services
                .AddScoped<ApplicationVersions>()
                .AddScoped<AdminServiceBehavior>()
                .AddScoped<ApplicationServiceBehavior>()
                .AddScoped<ApplicationFactServiceBehavior>();

            // (5) Add base Dependencies
            services
                .AddScoped<ApplicationSettingsServiceDependencies>();

            // (6) TODO: Add API services
            // services.AddScoped<AdminUsersService>();

            // (7) Default services
            services.TryAddSingleton<IIdentityProxy, NullIdentityProxy>();

            return services;
        }
    }
}
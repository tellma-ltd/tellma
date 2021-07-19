using Microsoft.Extensions.Configuration;
using System;
using Tellma.Instances;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InstancesExtensions
    {
        private const string SectionName = "Instances";

        /// <summary>
        /// Adds services that manage the assignment of every tenantId to exactly one web server instance. <br/>
        /// Useful when hosting the application on Azure app service or on a server farm and you want to run a
        /// background job in the web server per tenant without running it twice for the same tenant.
        /// </summary>
        public static IServiceCollection AddInstances(this IServiceCollection services, IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Add configuration
            var instancesSection = config.GetSection(SectionName);
            services.Configure<InstancesOptions>(instancesSection);

            // Register Services
            services = services
                .AddSingleton<InstanceInfoProvider>() // Tells you which tenants are under the management of the current instance
                .AddHostedService<HeartbeatJob>() // Keeps the tenants under the management of the current instance
                .AddHostedService<OrphanCareJob>(); // Brings any unmanaged tenants under the management of the current instance

            // Return
            return services;
        }
    }
}

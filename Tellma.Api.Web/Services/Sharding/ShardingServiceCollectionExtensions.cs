using Tellma.Services.Sharding;
using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ShardingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the services that makes the application sharding aware
        /// </summary>
        public static IServiceCollection AddSharding(this IServiceCollection services, IConfiguration configSection = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configSection != null)
            {
                // Add configuration
                services.Configure<ShardResolverOptions>(configSection);
            }

            services.AddSingleton<IShardResolver, ShardResolver>();

            return services;
        }
    }
}

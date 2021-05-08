using Microsoft.Extensions.Configuration;
using System;
using Tellma.Utilities.Sharding;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ShardingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the services that makes the application sharding aware
        /// </summary>
        public static ShardingBuilder AddSharding(this IServiceCollection services, IConfiguration configSection = null)
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

            services.AddSingleton<ICachingShardResolver, CachingShardResolver>();

            return new ShardingBuilder(services);
        }
    }
}

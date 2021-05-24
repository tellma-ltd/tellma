using Microsoft.Extensions.Configuration;
using System;
using Tellma.Utilities.Sharding;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ShardingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the services needed for the sharding infrastructure.
        /// </summary>
        public static ShardingBuilder AddSharding(this IServiceCollection services, IConfiguration configSection = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configSection is not null)
            {
                // Configure the options
                services.Configure<ShardResolverOptions>(configSection);
            }

            // Add services
            services.AddSingleton<ICachingShardResolver, CachingShardResolver>();

            return new ShardingBuilder(services);
        }
    }
}

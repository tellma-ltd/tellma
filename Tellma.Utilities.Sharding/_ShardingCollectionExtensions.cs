using Microsoft.Extensions.Configuration;
using System;
using Tellma.Utilities.Sharding;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ShardingCollectionExtensions
    {
        private const string SectionName = "Sharding";

        /// <summary>
        /// Registers the services needed for the sharding infrastructure.
        /// </summary>
        public static ShardingBuilder AddSharding(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Add configuration
            var shardingSection = config.GetSection(SectionName);
            services.Configure<ShardResolverOptions>(shardingSection);

            // Add services
            services.AddSingleton<IShardResolver, CachingShardResolver>();

            return new ShardingBuilder(services);
        }
    }
}

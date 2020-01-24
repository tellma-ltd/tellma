using Tellma.Services.Sharding;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ShardingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the services that makes the application sharding aware
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSharding(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IShardResolver, ShardResolver>();

            return services;
        }

    }
}

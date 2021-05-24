using Microsoft.Extensions.DependencyInjection;

namespace Tellma.Utilities.Sharding
{
    /// <summary>
    /// Helper functions for configuring Sharding.
    /// </summary>
    public class ShardingBuilder
    {
        private readonly IServiceCollection _services;

        internal ShardingBuilder(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Adds the <see cref="IConnectionResolver"/> service used by the sharding infrastructure to retrieve the database connection info.
        /// </summary>
        public ShardingBuilder AddConnectionResolver<TConnectionResolver>() where TConnectionResolver : class, IConnectionResolver
        {
            _services.AddSingleton<IConnectionResolver, TConnectionResolver>();

            return this;
        }
    }
}

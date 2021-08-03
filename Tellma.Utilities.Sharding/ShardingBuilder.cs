using Tellma.Utilities.Sharding;

namespace Microsoft.Extensions.DependencyInjection
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
        /// Adds the <see cref="IConnectionInfoLoader"/> service used by the sharding infrastructure to retrieve the database connection info.
        /// </summary>
        public ShardingBuilder AddConnectionResolver<TConnectionResolver>() where TConnectionResolver : class, IConnectionInfoLoader
        {
            _services.AddSingleton<IConnectionInfoLoader, TConnectionResolver>();

            return this;
        }
    }
}

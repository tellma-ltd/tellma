using Microsoft.Extensions.DependencyInjection;

namespace Tellma.Utilities.Sharding
{
    public class ShardingBuilder
    {
        private readonly IServiceCollection _services;

        internal ShardingBuilder(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Adds the <see cref="IShardResolver"/> services used by the sharding infrastructure to retrieve the database connection info 
        /// </summary>
        public ShardingBuilder AddShardResolver<T>() where T : class, IShardResolver
        {
            _services.AddSingleton<IShardResolver, T>();

            return this;
        }
    }
}

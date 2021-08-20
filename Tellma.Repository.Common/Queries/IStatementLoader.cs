using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Common;

namespace Tellma.Repository.Common
{
    public interface IStatementLoader
    {
        /// <summary>
        /// Loads a dynamic statement into a list of <see cref="DynamicRow"/>s.
        /// </summary>
        /// <param name="connString">The connection string to the SQL database from which to load the rows.</param>
        /// <param name="args">All the information needed to load the rows.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public Task<DynamicOutput> LoadDynamic(string connString, DynamicLoaderArguments args, CancellationToken cancellation = default);

        /// <summary>
        /// Loads a list of entity statements into a single principal list of entities, assumes that one of
        /// the entity statements is the root/principal statement, and all other entities are returned
        /// via the navigation and collection properties of the principal list.
        /// </summary>
        /// <typeparam name="TEntity">The type of the principal entities.</typeparam>
        /// <param name="connString">The connection string to the SQL database from which to load the entities.</param>
        /// <param name="args">All the information needed to load the entities.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public Task<EntityOutput<TEntity>> LoadEntities<TEntity>(string connString, EntityLoaderArguments args, CancellationToken cancellation = default) where TEntity : Entity;
    }
}

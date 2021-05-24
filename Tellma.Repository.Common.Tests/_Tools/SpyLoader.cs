using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Common;

namespace Tellma.Repository.Common.Tests
{
    /// <summary>
    /// Fake <see cref="IStatementLoader"/> that captures the results
    /// </summary>
    internal class SpyLoader : IStatementLoader
    {
        public DynamicLoaderArguments DynamicArgs { get; private set; }

        public EntityLoaderArguments EntityArgs { get; private set; }

        public Task<DynamicResult> LoadDynamic(string connString, DynamicLoaderArguments args, CancellationToken cancellation = default)
        {
            DynamicArgs = args;
            var result = new DynamicResult(null, null, 0);
            return Task.FromResult(result);
        }

        public Task<EntityResult<TEntity>> LoadEntities<TEntity>(string connString, EntityLoaderArguments args, CancellationToken cancellation = default) where TEntity : Entity
        {
            EntityArgs = args;
            var fakeResult = new EntityResult<TEntity>(null, 0);
            return Task.FromResult(fakeResult);
        }
    }
}

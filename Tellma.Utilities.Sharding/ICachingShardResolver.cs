using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Utilities.Sharding
{
    /// <summary>
    /// Resolves the connection string of a specific database in a multitenant environment where every tenant uses a separate database
    /// </summary>
    public interface ICachingShardResolver
    {
        /// <summary>
        /// Returns the connection string that can be used to reach a certain application database.
        /// </summary>
        /// <param name="databaseId">The Id of the application database.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        Task<string> GetConnectionString(int databaseId, CancellationToken cancellation);
    }
}

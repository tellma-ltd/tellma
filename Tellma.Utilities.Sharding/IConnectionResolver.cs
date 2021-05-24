using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Utilities.Sharding
{
    /// <summary>
    /// Services that implement this interface are able to retrieve connection information of a
    /// certain database in a multitenant environment where every tenant uses a separate database.
    /// <para/>
    /// The connection information is packaged as a <see cref="DatabaseConnectionInfo"/>.
    /// </summary>
    public interface IConnectionResolver
    {
        /// <summary>
        /// Retrieves connection information of a specific application database in a
        /// multitenant environment where every tenant uses a separate database.
        /// </summary>
        /// <param name="databaseId">The Id of the application databse.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public Task<DatabaseConnectionInfo> Resolve(int databaseId, CancellationToken cancellation);
    }
}

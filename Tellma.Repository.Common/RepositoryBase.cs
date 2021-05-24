using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Base class for all repositories.
    /// </summary>
    public abstract class RepositoryBase
    {
        protected abstract ILogger Logger { get; }

        /// <summary>
        /// Helper function that executes a block of code containing a DB call, with
        /// retry logic if the code throws a transient <see cref="SqlException"/>.
        /// </summary>
        protected Task ExponentialBackoff(Func<Task> spCall, string spName, CancellationToken cancellation = default) =>
            RepositoryUtilities.ExponentialBackoff(spCall, Logger, $"{GetType().Name}.{spName}", cancellation);

        /// <summary>
        /// Determines whether the given <see cref="SqlException"/> is a foreign key violation on delete.
        /// </summary>
        protected static bool IsForeignKeyViolation(SqlException ex) => 
            RepositoryUtilities.IsForeignKeyViolation(ex);
    }
}

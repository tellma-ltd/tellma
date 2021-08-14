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
        /// retry logic if the code throws a transient <see cref="SqlException"/>. <br/>
        /// The block of code is wrapped inside a transaction with a read-committed isolation level.
        /// </summary>
        protected async Task TransactionalDatabaseOperation(Func<Task> spCall, string dbName, string spName, CancellationToken cancellation = default)
        {
            using var trx = TransactionFactory.ReadCommitted();
            
            await RepositoryUtilities.ExponentialBackoff(spCall, Logger, dbName, spName, cancellation);

            trx.Complete();
        }

        /// <summary>
        /// Helper function that executes a block of code containing a DB call, with
        /// retry logic if the code throws a transient <see cref="SqlException"/>.
        /// </summary>
        protected async Task ExponentialBackoff(Func<Task> spCall, string dbName, string spName, CancellationToken cancellation = default)
        {
            await RepositoryUtilities.ExponentialBackoff(spCall, Logger, dbName, spName, cancellation);
        }

        /// <summary>
        /// Determines whether the given <see cref="SqlException"/> is a foreign key violation on delete.
        /// </summary>
        protected static bool IsForeignKeyViolation(SqlException ex) => 
            RepositoryUtilities.IsForeignKeyViolation(ex);
    }
}

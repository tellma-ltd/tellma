using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Model.Common;

namespace Tellma.Repository.Common
{
    public static class RepositoryUtilities
    {
        private static readonly Random _rand = new();

        #region Data Tables

        /// <summary>
        /// Constructs a SQL data table containing all the public properties of the 
        /// entities' type and populates the data table with the provided entities.
        /// This function automatically adds index columns for self referencing properties
        /// </summary>
        public static DataTable DataTable<T>(IEnumerable<T> entities, bool addIndex = false, IEnumerable<ExtraColumn<T>> extraColumns = null) where T : Entity
        {
            var table = new DataTable();
            if (addIndex)
            {
                // The column order MUST match the column order in the user-defined table type
                table.Columns.Add(new DataColumn("Index", typeof(int)));
            }

            var props = AddColumnsFromProperties(table, extraColumns);

            int index = 0;
            foreach (var entity in entities)
            {
                if (entity != null)
                {
                    DataRow row = table.NewRow();

                    // We add an index property since SQL works with un-ordered sets
                    if (addIndex)
                    {
                        row["Index"] = index;
                    }

                    // Add the remaining properties
                    foreach (var prop in props)
                    {
                        if (prop.IsSelfReferencing)
                        {
                            object indexValue = prop.GetIndexProperty(entity);
                            row[prop.IndexPropertyName] = indexValue ?? DBNull.Value;
                        }

                        var propValue = prop.GetValue(entity);
                        row[prop.Name] = propValue ?? DBNull.Value;
                    }

                    // Custom columns
                    if (extraColumns != null)
                    {
                        foreach (var extra in extraColumns)
                        {
                            var propValue = extra.GetValue(entity);
                            row[extra.Name] = propValue ?? DBNull.Value;
                        }
                    }

                    table.Rows.Add(row);
                }

                index++;
            }

            return table;
        }

        public static DataTable DataTableWithHeaderIndex<THeader, TLines>(IEnumerable<THeader> entities, Func<THeader, List<TLines>> linesFunc, IEnumerable<ExtraColumn<TLines>> extraColumns = null) where THeader : Entity where TLines : Entity
        {
            var table = new DataTable();

            // The column order MUST match the column order in the user-defined table type
            table.Columns.Add(new DataColumn("Index", typeof(int)));
            table.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));

            var props = AddColumnsFromProperties(table, extraColumns);

            int headerIndex = 0;
            foreach (var entity in entities)
            {
                int index = 0;
                var lines = linesFunc(entity);
                if (lines != null)
                {
                    foreach (var line in linesFunc(entity))
                    {
                        DataRow row = table.NewRow();

                        // We add an index property since SQL works with un-ordered sets
                        row["Index"] = index++;
                        row["HeaderIndex"] = headerIndex;

                        // Add the remaining properties
                        foreach (var prop in props)
                        {
                            if (prop.IsSelfReferencing)
                            {
                                // This will probably never be used, we don't have self referencing properties on weak entities
                                object indexValue = prop.GetIndexProperty(entity);
                                row[prop.IndexPropertyName] = indexValue ?? DBNull.Value;
                            }

                            var propValue = prop.GetValue(line);
                            row[prop.Name] = propValue ?? DBNull.Value;
                        }

                        // Custom columns
                        if (extraColumns != null)
                        {
                            foreach (var extra in extraColumns)
                            {
                                var propValue = extra.GetValue(line);
                                row[extra.Name] = propValue ?? DBNull.Value;
                            }
                        }

                        table.Rows.Add(row);
                    }
                }

                headerIndex++;
            }

            return table;
        }

        public static IEnumerable<PropertyDescriptor> AddColumnsFromProperties<T>(DataTable table, IEnumerable<ExtraColumn<T>> extras = null) where T : Entity
        {
            var props = TypeDescriptor.Get<T>().SimpleProperties;
            foreach (var prop in props)
            {
                // If it's a self referencing FK column, add the index first (by convention the index column immediate precedes the self ref FK column
                if (prop.IsSelfReferencing)
                {
                    var indexColumn = new DataColumn(prop.IndexPropertyName, typeof(int));
                    table.Columns.Add(indexColumn);
                }

                // Add the column itself
                var propType = Nullable.GetUnderlyingType(prop.Type) ?? prop.Type;
                var column = new DataColumn(prop.Name, propType);
                if (propType == typeof(string))
                {
                    // For string columns, it is more performant to explicitly specify the maximum column size
                    // According to this article: http://www.dbdelta.com/sql-server-tvp-performance-gotchas/
                    int maxLength = prop.MaxLength;
                    if (maxLength > 0)
                    {
                        column.MaxLength = maxLength;
                    }
                }

                table.Columns.Add(column);
            }

            if (extras != null)
            {
                foreach (var extra in extras)
                {
                    var column = new DataColumn(extra.Name, extra.Type);
                    table.Columns.Add(column);
                }
            }

            return props;
        }

        public static ExtraColumn<T> Column<T>(string name, Type type, Func<T, object> getValue)
        {
            return new ExtraColumn<T>
            {
                Name = name,
                Type = type,
                GetValue = getValue
            };
        }

        #endregion

        #region Fault Tolerance

        /// <summary>
        /// Helper function that executes a block of code containing a DB call, with
        /// retry logic if the code throws a transient <see cref="SqlException"/>.
        /// </summary>
        public static async Task ExponentialBackoff(
            Func<Task> operation,
            ILogger logger,
            string dbName,
            string spName,
            CancellationToken cancellation = default)
        {
            // Exponential backoff
            const int eventId = 1001;
            const int maxAttempts = 3;
            const int maxBackoff = 4000; // 4 Seconds
            const int minBackoff = 1000; // 1 Second
            const int deltaBackoff = 1000; // 1 Second

            int attemptsSoFar = 0;
            int backoff = minBackoff;
            Guid? queryId = null;  // To group attempts of a single query together

            while (attemptsSoFar < maxAttempts && !cancellation.IsCancellationRequested)
            {
                attemptsSoFar++;
                try
                {
                    Stopwatch sw = new();
                    sw.Start();

                    // Execute Query
                    await operation();

                    // Log the time
                    sw.Stop();
                    RunOutsideTransaction(() => logger.LogTrace(eventId,
                        "[{dbName}].[{spName}]: Completed in {milliseconds} ms.",
                        dbName, spName, sw.ElapsedMilliseconds));

                    // Break the cycle if successful
                    break;
                }
                catch (Exception ex) when (IsTransient(ex) && attemptsSoFar < maxAttempts) // Only transient errors should be handled by retry logic
                {
                    queryId ??= Guid.NewGuid();
                    var randomOffset = _rand.Next(0, deltaBackoff);
                    var retryIn = backoff + randomOffset;

                    // Log a warning
                    RunOutsideTransaction(() => logger.LogWarning(ex,
                            "[{dbName}].[{spName}]: Failed after {attemptsSoFar}/{maxAttempts} attempts, retrying in {retryIn}. Operation ID: {queryId:N}.",
                            dbName, spName, attemptsSoFar, maxAttempts, retryIn, queryId));
                    
                    // Exponential backoff
                    await Task.Delay(retryIn, cancellation);

                    // Double the backoff for next attempt without exceeding maxBackoff
                    backoff = Math.Min(backoff * 2, maxBackoff);
                }
            }
        }

        /// <summary>
        /// Runs an <paramref name="action"/> outside any ambient transactions. Used for logging warnings.
        /// </summary>
        /// <param name="action">The action to run outside any ambient transaction.</param>
        private static void RunOutsideTransaction(Action action)
        {
            using var trx = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
            action();
            trx.Complete();
        }

        #endregion

        #region Exceptions

        /// <summary>
        /// Determines if the Exception thrown during a db operation is transient, ie if it
        /// is likely to disappear on its own if we try the operation again after a short delay.
        /// <para/>
        /// The Exception numbers were collected from these resources: <br/>
        /// - https://bit.ly/3y0HQmT <br/>
        /// - https://bit.ly/3vTnbiK <br/>
        /// - https://bit.ly/3ewpRNG <br/>
        /// - https://bit.ly/2Q3yupv <br/>
        /// - https://bit.ly/3uA8SPV <br/>
        /// - https://bit.ly/3o0QaP3 <br/>
        /// </summary>
        public static bool IsTransient(Exception ex)
        {
            if (ex is SqlException sqlException)
            {
                return sqlException.IsTransient || sqlException.Errors.Cast<SqlError>().Any(err => err.Number switch
                {
                    49920 => true, // Cannot process request. Too many operations in progress for subscription "%ld".
                    49919 => true, // Cannot process create or update request. Too many create or update operations in progress for subscription "%ld".
                    49918 => true, // Cannot process request. Not enough resources to process request.
                    41839 => true, // Transaction exceeded the maximum number of commit dependencies and the last statement was aborted. Retry the statement.
                    41325 => true, // The current transaction failed to commit due to a serializable validation failure.
                    41305 => true, // The current transaction failed to commit due to a repeatable read validation failure.
                    41302 => true, // The current transaction attempted to update a record that has been updated since this transaction started. The transaction was aborted.
                    41301 => true, // A previous transaction that the current transaction took a dependency on has aborted, and the current transaction can no longer commit.
                    40613 => true, // Database '%.*ls' on server '%.*ls' is not currently available. Please retry the connection later.
                    40550 => true, // The session has been terminated because it has acquired too many locks.
                    40549 => true, // Session is terminated because you have a long-running transaction
                    40540 => true, // Transaction was aborted as database is moved to read-only mode. This is a temporary situation and please retry the operation.
                    40501 => true, // The service is currently busy. Retry the request after 10 seconds. Incident ID: %ls. Code: %d.
                    40197 => true, // The service has encountered an error processing your request. Please try again. Error code %d.
                    40143 => true, // The service has encountered an error processing your request. Please try again.
                    18401 => true, // Login failed for user '%s'. Reason: Server is in script upgrade mode. Only administrator can connect at this time.
                    12015 => true, // The index option %.*ls in the CREATE %S_MSG statement has to appear before the general index options.
                    11001 => sqlException.State == 0, // An error has occurred while establishing a connection to the server.
                    10936 => true, // Resource ID : %d. The %ls limit for the elastic pool is %d and has been reached.
                    10929 => true, // Resource ID : %d. The %s minimum guarantee is %d, maximum limit is %d and the current usage for the database is %d....
                    10928 => true, // Resource ID : %d. The %ls limit for the database is %d and has been reached.
                    10060 => sqlException.State == 0, // An error has occurred while establishing a connection to the server. 
                    10054 => sqlException.State == 0, // An existing connection was forcibly closed by the remote host.
                    10053 => sqlException.State == 0, // A transport-level error has occurred when receiving results from the server.
                    4221 => true, // Login to read-secondary failed due to long wait on 'HADR_DATABASE_WAIT_FOR_TRANSITION_TO_VERSIONING'.
                    4060 => true, // Cannot open database "%.*ls" requested by the login. The login failed.
                    1205 => true, // Transaction (Process ID %d) was deadlocked on %.*ls resources with another process and has been chosen as the deadlock victim.
                    233 => true, // A connection was successfully established with the server, but then an error occurred during the login process.
                    121 => sqlException.State == 0, // TCP Provider: The semaphore timeout period has expired.
                    64 => true, // Transport-level error has occurred when receiving results from the server.
                    20 => true, // Encryption capability mismatch.
                    -2 => true, // Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.
                    _ => false,
                });
            }
            else if (ex is TimeoutException)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the given <see cref="SqlException"/> is a foreign key violation on delete.
        /// </summary>
        public static bool IsForeignKeyViolation(SqlException ex) => ex.Number is 547;

        /// <summary>
        /// Determines whether the given <see cref="SqlException"/> is a divide by zero error.
        /// </summary>
        public static bool IsDivideByZeroException(SqlException ex) => ex.Number is 8134;

        #endregion
    }

    public class ExtraColumn<T>
    {
        /// <summary>
        /// Column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Column type (don't use nullable).
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Function that gets the value
        /// </summary>
        public Func<T, object> GetValue { get; set; }
    }
}

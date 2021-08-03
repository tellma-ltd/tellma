using Microsoft.Extensions.Logging;
using System;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// All the information required by <see cref="EntityQuery{T}"/>, <see cref="FactQuery{T}"/> and <see cref="AggregateQuery{T}"/> to compile to SQL and load data.
    /// </summary>
    public class QueryArguments
    {
        public QueryArguments(Func<Type, string> sources, string connString, IStatementLoader loader)
        {
            Sources = sources ?? throw new ArgumentNullException(nameof(sources));
            ConnectionString = connString ?? throw new ArgumentNullException(nameof(connString));
            Loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        /// <summary>
        /// Mapping from every .NET type to the SQL expression that it can be selected from.
        /// </summary>
        public Func<Type, string> Sources { get; }

        /// <summary>
        /// The connection string to the database on which to execute the query.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// An implementation of <see cref="ILogger"/> to log warnings and errors during query execution.
        /// </summary>
        public IStatementLoader Loader { get; }
    }
}

using System;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Defines a column that is returned by executing an <see cref="SqlStatement"/>,
    /// an ordered list of <see cref="SqlStatementColumn"/> define the query results.
    /// For example "SELECT [P].[Name], SUM([P1].[Amount])" can be encoded as two <see cref="SqlStatementColumns"/>s
    /// </summary>
    public class SqlStatementColumn
    {
        /// <summary>
        /// The path from the root type leading to this property
        /// </summary>
        public ArraySegment<string> Path { get; set; }

        /// <summary>
        /// The property that the column is returning
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// The function applied on the column if any (e.g. dayofyear)
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// The aggregation function applied on the column if any (e.g. sum)
        /// </summary>
        public string Aggregation { get; set; }
    }
}

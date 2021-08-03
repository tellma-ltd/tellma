using System;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Defines a column that is returned by executing an <see cref="SqlEntityStatement"/>,
    /// an ordered list of <see cref="SqlEntityStatementColumn"/> define the query results.
    /// For example "SELECT [P].[Name], [P1].[Amount]" can be encoded as two <see cref="SqlStatementColumns"/>s
    /// </summary>
    public class SqlEntityStatementColumn
    {
        /// <summary>
        /// The path from the root type leading to this property
        /// </summary>
        public ArraySegment<string> Path { get; set; }

        /// <summary>
        /// The property that the column is returning
        /// </summary>
        public string Property { get; set; }
    }
}

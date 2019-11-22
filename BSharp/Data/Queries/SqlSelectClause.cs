using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Represents a SELECT clause in a flat SQL query (no GROUP BY), it has methods for constructing
    /// the actual SQL SELECT and the definitions of the columns returned by that SQL SELECT 
    /// </summary>
    public class SqlSelectClause
    {
        private readonly List<(string Symbol, ArraySegment<string> Path, string PropName)> _columns;

        /// <summary>
        /// Create a new instance of <see cref="SqlSelectClause"/> using the supplied column definitions
        /// </summary>
        public SqlSelectClause(List<(string Symbol, ArraySegment<string> Path, string PropName)> columns)
        {
            _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        }

        /// <summary>
        /// Constructs and returns the SQL SELECT clause string corresponding to this <see cref="SqlSelectClause"/>
        /// </summary>
        public string ToSql(bool isAncestorExpand)
        {
            string distinct = isAncestorExpand ? "DISTINCT " : "";
            return $"SELECT {distinct}" + string.Join(", ", _columns.Select(e => $"[{e.Symbol}].[{e.PropName}]"));
        }

        /// <summary>
        /// Returns a list of <see cref="SqlStatementColumn"/> which define the columns that are returned by this <see cref="SqlSelectClause"/> in the correct order
        /// </summary>
        public List<SqlStatementColumn> GetColumnMap()
        {
            // Prepare the column map
            var columnMap = _columns.Select(e => new SqlStatementColumn
            {
                Path = e.Path,
                Property = e.PropName,
            })
            .ToList();

            return columnMap;
        }
    }
}

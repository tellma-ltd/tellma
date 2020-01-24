using System;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents both a SELECT and a GROUP BY clauses of an aggregate SQL query, it has methods for constructing
    /// the actual SQL SELECT and GROUP BY clauses and the definitions of the columns returned by the SELECT clause
    /// </summary>
    public class SqlSelectGroupByClause
    {
        private readonly int _top;
        private readonly List<(string Symbol, ArraySegment<string> Path, string PropName, string Aggregation, string Function)> _columns;

        /// <summary>
        /// Create a new instance of <see cref="SqlSelectGroupByClause"/> using the supplied column definitions
        /// </summary>
        public SqlSelectGroupByClause(List<(string Symbol, ArraySegment<string> Path, string PropName, string Aggregation, string Function)> columns, int top)
        {
            _top = top;
            _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        }

        /// <summary>
        /// Constructs and returns the SQL SELECT clause string corresponding to this <see cref="SqlSelectGroupByClause"/>
        /// </summary>
        public string ToSelectSql()
        {
            string top = _top == 0 ? "" : $"TOP {_top} ";
            return $"SELECT {top}" + string.Join(", ", _columns.Select(e => QueryTools.AtomSql(e.Symbol, e.PropName, e.Aggregation, e.Function)));
        }

        /// <summary>
        /// Constructs and returns the SQL GROUP BY clause string corresponding to this <see cref="SqlSelectGroupByClause"/>
        /// </summary>
        public string ToGroupBySql()
        {
            // take all columns that are not aggregated, and group them together
            var nonAggregateSelects = _columns.Where(e => string.IsNullOrWhiteSpace(e.Aggregation));
            if (nonAggregateSelects.Any())
            {
                return "GROUP BY " + string.Join(", ", nonAggregateSelects.Select(e => QueryTools.AtomSql(e.Symbol, e.PropName, e.Aggregation, e.Function)));
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Returns a list of <see cref="SqlStatementColumn"/> which define the columns that are returned by this <see cref="SqlSelectGroupByClause"/> in the correct order
        /// </summary>
        public List<SqlStatementColumn> GetColumnMap()
        {
            // Prepare the column map
            var columnMap = _columns.Select(e => new SqlStatementColumn
            {
                Path = e.Path,
                Property = e.PropName,
                Aggregation = e.Aggregation,
                Function = e.Function
            })
            .ToList();

            return columnMap;
        }
    }
}

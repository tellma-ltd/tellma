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
        private readonly QxCompilationContext _ctx;
        private readonly List<QueryexBase> _columns;

        /// <summary>
        /// Create a new instance of <see cref="SqlSelectGroupByClause"/> using the supplied column definitions
        /// </summary>
        public SqlSelectGroupByClause(List<QueryexBase> columns, int top, QxCompilationContext ctx)
        {
            _top = top;
            _ctx = ctx;
            _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        }

        /// <summary>
        /// Constructs and returns the SQL SELECT clause string corresponding to this <see cref="SqlSelectGroupByClause"/>
        /// </summary>
        public string ToSelectSql()
        {
            string top = _top == 0 ? "" : $"TOP {_top} ";
            return $"SELECT {top}" + string.Join(", ", _columns.Select(e => e.CompileToNonBoolean(_ctx)));
        }

        /// <summary>
        /// Constructs and returns the SQL GROUP BY clause string corresponding to this <see cref="SqlSelectGroupByClause"/>
        /// </summary>
        public string ToGroupBySql()
        {
            // take all columns that are not aggregated, and group them together
            var nonAggregateSelects = _columns.Where(e => !e.ContainsAggregations);
            if (nonAggregateSelects.Any())
            {
                return "GROUP BY " + string.Join(", ", nonAggregateSelects.Select(e => e.CompileToNonBoolean(_ctx)));
            }
            else
            {
                return "";
            }
        }
    }
}

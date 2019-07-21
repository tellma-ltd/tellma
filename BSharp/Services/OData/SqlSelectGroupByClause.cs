using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class SqlSelectGroupByClause
    {
        private readonly int _top;
        private readonly List<(string Symbol, ArraySegment<string> Path, string PropName, string Aggregation)> _columns;

        public SqlSelectGroupByClause(List<(string Symbol, ArraySegment<string> Path, string PropName, string Aggregation)> columns, int top)
        {
            _top = top;
            _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        }

        public string ToSelectSql()
        {
            string top = _top == 0 ? "" : $"TOP {_top} ";
            return $"SELECT {top}" + string.Join(", ", _columns.Select(e => ODataTools.AtomSql(e.Symbol, e.PropName, e.Aggregation)));
        }

        public string ToGroupBySql()
        {
            // take all columns that are not aggregated, and group them together
            var nonAggregateSelects = _columns.Where(e => string.IsNullOrWhiteSpace(e.Aggregation));
            return "GROUP BY " + string.Join(", ", nonAggregateSelects.Select(e => ODataTools.AtomSql(e.Symbol, e.PropName, e.Aggregation)));
        }

        public List<SqlStatementColumn> GetColumnMap()
        {
            // Prepare the column map
            var columnMap = _columns.Select(e => new SqlStatementColumn
            {
                Path = e.Path,
                Property = e.PropName,
                Aggregation = e.Aggregation
            })
            .ToList();

            return columnMap;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class SqlSelectClause
    {
        private readonly List<(string Symbol, ArraySegment<string> Path, string PropName)> _columns;

        public SqlSelectClause(List<(string Symbol, ArraySegment<string> Path, string PropName)> columns)
        {
            _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        }

        public string ToSql()
        {
            return "SELECT " + string.Join(", ", _columns.Select(e => $"[{e.Symbol}].[{e.PropName}]"));
        }

        public List<SqlStatementColumn> GetColumnMap()
        {
            // Prepare the column map
            var columnMap = _columns.Select(e => new SqlStatementColumn
            {
                Path = e.Path,
                Property = e.PropName
            })
            .ToList();

            return columnMap;
        }
    }
}

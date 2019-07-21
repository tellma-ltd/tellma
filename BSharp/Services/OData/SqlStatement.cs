using System;
using System.Collections.Generic;

namespace BSharp.Services.OData
{
    public class SqlStatement
    {
        public string Sql { get; set; }

        public Type ResultType { get; set; }

        /// <summary>
        /// Maps every column index to the path and property
        /// </summary>
        public List<SqlStatementColumn> ColumnMap { get; set; }

        public IQueryInternal Query { get; set; }

        public bool IsAggregate { get; set; }
    }
}

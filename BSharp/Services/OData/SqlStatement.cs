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
        public List<(ArraySegment<string> Path, string Property)> ColumnMap { get; set; }

        public ODataFlatQuery Query { get; set; }
    }
}

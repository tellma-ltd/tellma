using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public class SqlStatementColumn
    {
        public ArraySegment<string> Path { get; set; }

        public string Property { get; set; }

        public string Aggregation { get; set; }
    }
}

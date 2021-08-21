using System;
using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Api.Dto
{
    public class GetAggregateResponse
    {
        /// <summary>
        /// Contains the result of the aggregate query.
        /// </summary>
        public IEnumerable<DynamicRow> Result { get; set; }

        /// <summary>
        /// Contains the ancestors of tree dimensions if any.
        /// </summary>
        public IEnumerable<DimensionAncestors> DimensionAncestors { get; set; }

        public DateTimeOffset ServerTime { get; set; }
    }

    public class DimensionAncestors
    {
        /// <summary>
        /// The id of the index, clients use this value to identify which tree dimension
        /// this represents if there were multiple of them in the same query.
        /// </summary>
        public int IdIndex { get; set; }

        /// <summary>
        /// Column index i from <see cref="Result"/> maps to column index i + <see cref="MinIndex"/> 
        /// in the principal result.
        /// </summary>
        public int MinIndex { get; set; }

        /// <summary>
        /// The dynamic rows of the dimension ancestors from the DB.
        /// </summary>
        public List<DynamicRow> Result { get; set; }
    }
}

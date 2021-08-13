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
        public IEnumerable<DimensionAncestorsResult> DimensionAncestors { get; set; }

        public DateTimeOffset ServerTime { get; set; }

        public bool IsPartial { get; set; }
    }
}

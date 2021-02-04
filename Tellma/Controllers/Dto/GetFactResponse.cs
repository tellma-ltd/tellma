using System;
using System.Collections;
using System.Collections.Generic;
using Tellma.Data.Queries;

namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// The resonse for <see cref="FactControllerBase{TEntity}.GetFact(GetArguments, System.Threading.CancellationToken)"/>
    /// which returns a flat result in the form of an array of <see cref="DynamicRow"/>s
    /// </summary>
    public class GetFactResponse
    {
        /// <summary>
        /// Contains the result of the fact query
        /// </summary>
        public IEnumerable<DynamicRow> Result { get; set; }

        public DateTimeOffset ServerTime { get; set; }

        public bool IsPartial { get; set; }

        public int? TotalCount { get; set; }
    }
}

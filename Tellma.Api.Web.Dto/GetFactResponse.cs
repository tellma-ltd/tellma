using System;
using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Api.Dto
{
    /// <summary>
    /// The response for <see cref="FactControllerBase{TEntity}.GetFact(GetArguments, System.Threading.CancellationToken)"/>
    /// which returns a flat result in the form of an array of <see cref="DynamicRow"/>s.
    /// </summary>
    public class GetFactResponse
    {
        /// <summary>
        /// Contains the result of the fact query
        /// </summary>
        public IEnumerable<DynamicRow> Result { get; set; }

        public DateTimeOffset ServerTime { get; set; }

        public int? TotalCount { get; set; }
    }
}

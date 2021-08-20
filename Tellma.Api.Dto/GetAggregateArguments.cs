namespace Tellma.Api.Dto
{
    /// <summary>
    /// Carries the parameters for service requests that query entities in an aggregate fashion.
    /// </summary>
    public class GetAggregateArguments
    {
        /// <summary>
        /// Specifies the number of aggregation rows to return.
        /// </summary>
        public int Top { get; set; } = 0;

        /// <summary>
        /// A comma separated list of Queryex-style expressions to order the aggregation rows by.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// A Queryex-style boolean expression used to filter the source data before aggregating it.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// A Queryex-style boolean expression used to filter the aggregation rows.
        /// </summary>
        public string Having { get; set; }

        /// <summary>
        /// A comma separated list of Queryex-style expressions that can either be aggregated (measures)
        /// or non aggregated (dimensions).
        /// </summary>
        /// <remarks>
        /// - An aggregated expression encloses every column-access in an aggregation function, example: Sum(Amount).<br/>
        /// - A non-aggregated atom contains no aggregation functions, example: "Resource.Name".
        /// </remarks>
        public string Select { get; set; }
    }
}

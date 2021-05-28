namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// Carries the parameters for service requests that query entities in an aggregate fashion.
    /// </summary>
    public class GetAggregateArguments
    {
        /// <summary>
        /// Specifies the number of rows that the server should return.
        /// </summary>
        public int Top { get; set; } = 0;

        /// <summary>
        /// The expression to order the results by.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// An OData-style filter string that enables a rich query experience.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// An OData-style filter string that is applied after the aggregation, enablign a rich query experience.
        /// </summary>
        public string Having { get; set; }

        /// <summary>
        /// Equivalent to linq's "Select", but tailored for an aggregate query, the atoms can be
        /// either aggregated (measures) or non-aggregated (dimensions).<br/>
        /// An aggregated atom encloses every column access in an aggregation function like: Sum(Amount).<br/>
        /// A non-aggregated atom contains no aggregation functions like: "Resource.Name".
        /// </summary>
        public string Select { get; set; }
    }
}

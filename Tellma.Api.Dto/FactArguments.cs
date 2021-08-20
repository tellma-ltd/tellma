namespace Tellma.Api.Dto
{
    /// <summary>
    /// Arguments for a service request that returns fact data.
    /// </summary>
    public class FactArguments
    {
        public const int DefaultPageSize = 25;

        /// <summary>
        /// A comma separated list of Queryex expressions to select from the data source.
        /// </summary>
        public string Select { get; set; }

        /// <summary>
        /// A comma separated list of Queryex expressions to order the results by.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// A single boolean Queryex expression to filter the data by.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Specifies how many rows to return, defaults to <see cref="DefaultPageSize"/>.
        /// </summary>
        public int Top { get; set; } = DefaultPageSize;

        /// <summary>
        /// Specifies how many rows to skip before the returned rows.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// When set to True, instructs the server to include the total count of the
        /// data without the constraint of <see cref="Top"/> and <see cref="Skip"/>. 
        /// </summary>
        /// <remarks>
        /// The server counts up to a 10000, or returns <see cref="int.MaxValue"/> if
        /// that limit is exceeded as a performance optimization.
        /// </remarks>
        public bool CountEntities { get; set; }
    }
}

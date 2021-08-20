namespace Tellma.Api.Dto
{
    /// <summary>
    /// Arguments for a service request that returns entities.
    /// </summary>
    public class GetArguments : SelectExpandArguments
    {
        public const int DefaultPageSize = 25;

        /// <summary>
        /// A search string that is interpreted in a custom way by every service.
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// A comma separated list of Queryex expressions to order the results by.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// A single boolean Queryex expression to filter the data by.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Specifies how many entities to return, defaults to <see cref="DefaultPageSize"/>.
        /// </summary>
        public int Top { get; set; } = DefaultPageSize;

        /// <summary>
        /// Specifies how many entities to skip before the returned entities.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// When set to True, instructs the server to include the total count of the
        /// entities without the constraint of <see cref="Top"/> and <see cref="Skip"/>. 
        /// </summary>
        /// <remarks>
        /// The server counts up to a 10000, or returns <see cref="int.MaxValue"/> if
        /// that limit is exceeded as a performance optimization.
        /// </remarks>
        public bool CountEntities { get; set; }
    }
}

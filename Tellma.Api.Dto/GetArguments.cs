namespace Tellma.Api.Dto
{
    /// <summary>
    /// Carries the parameters for service requests that query entities.
    /// </summary>
    public class GetArguments : SelectExpandArguments
    {
        private const int DEFAULT_PAGE_SIZE = 25;

        /// <summary>
        /// Specifies the number of items the server should return.
        /// Defaults to 25.
        /// </summary>
        public int Top { get; set; } = DEFAULT_PAGE_SIZE;

        /// <summary>
        /// Specifies how many items to skip before the returned collection.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// The expression to order the results by.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// A search string that is interpreted in a custom way by every service.
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// An OData-style filter string that enables a rich query experience.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// When set to true, instructs the server to include the total of the query
        /// (without the constraint of <see cref="Top"/> and <see cref="Skip"/>), the server
        /// counts results up to a certain maximum limit, or returns <see cref="int.MaxValue"/>
        /// if that limit is exceeded.
        /// </summary>
        public bool CountEntities { get; set; }
    }
}

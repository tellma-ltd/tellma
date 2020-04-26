namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// DTO that carries the parameters for most Http GET all requests
    /// </summary>
    public class GetArguments : SelectExpandArguments
    {
        private const int DEFAULT_PAGE_SIZE = 25;

        /// <summary>
        /// Specifies the number of items the server should return
        /// </summary>
        public int Top { get; set; } = DEFAULT_PAGE_SIZE;

        /// <summary>
        /// Specifies how many items to skip before the returned collection
        /// </summary>
        public int Skip { get; set; } = 0;

        /// <summary>
        /// The name of the property to order the result by
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// A search string that is interpreted in a customized way by every controller
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// An OData style filter string that is parsed into a Linq expression enabling a rich 
        /// query experience
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// When set to true, instructs the server to include the <see cref="GetResponse{TEntity}.TotalCount"/> of the query
        /// (without the constraint of <see cref="Top"/> and <see cref="Skip"/>), the server
        /// counts results up to a certain maximum limit <see cref="FactControllerBase{TEntity}.MAXIMUM_COUNT"/>, or returns <see cref="int.MaxValue"/>
        /// if that limit is exceeded
        /// </summary>
        public bool CountEntities { get; set; }
    }
}

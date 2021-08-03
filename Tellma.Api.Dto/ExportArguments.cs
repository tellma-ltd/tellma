namespace Tellma.Api.Dto
{
    public class ExportArguments
    {
        private const int DEFAULT_PAGE_SIZE = 10000;

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
    }
}

namespace Tellma.Api.Dto
{
    public class ExportArguments
    {
        private const int DEFAULT_PAGE_SIZE = 10000;

        /// <summary>
        /// Specifies the number of entities to export.
        /// </summary>
        public int Top { get; set; } = DEFAULT_PAGE_SIZE;

        /// <summary>
        /// Specifies how many entities to skip before the exported entities.
        /// </summary>
        public int Skip { get; set; } = 0;

        /// <summary>
        /// A comma separated list of Queryex-style expressions to order the exported entities by.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// A search string that is interpreted in a customized way by every service.
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// A Queryex-style filter used to filer the exported entities.
        /// </summary>
        public string Filter { get; set; }
    }
}

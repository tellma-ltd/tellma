using Tellma.Entities;

namespace Tellma.Controllers.Dto
{
    public class StatementResponse : EntitiesResponse<DetailsEntry>
    {
        /// <summary>
        /// Opening Balance of the result
        /// </summary>
        public decimal? Opening { get; set; }

        /// <summary>
        /// Closing Balance of the result
        /// </summary>
        public decimal? Closing { get; set; }

        public int Skip { get; set; }

        public int Top { get; set; }

        /// <summary>
        /// The total count of the result if it weren't paged
        /// </summary>
        public int TotalCount { get; set; }
    }
}

using Tellma.Model.Application;

namespace Tellma.Api.Dto
{
    public class StatementResponse : EntitiesResponse<DetailsEntry>
    {
        /// <summary>
        /// Opening Balance of the result.
        /// </summary>
        public decimal? Opening { get; set; }

        /// <summary>
        /// Opening Quantity Balance of the result.
        /// </summary>
        public decimal? OpeningQuantity { get; set; }

        /// <summary>
        /// Opening Monetary Value Balance of the result.
        /// </summary>
        public decimal? OpeningMonetaryValue { get; set; }

        /// <summary>
        /// Closing Balance of the result.
        /// </summary>
        public decimal? Closing { get; set; }

        /// <summary>
        /// Closing Quantity Balance of the result.
        /// </summary>
        public decimal? ClosingQuantity { get; set; }

        /// <summary>
        /// Closing Monetary Value Balance of the result.
        /// </summary>
        public decimal? ClosingMonetaryValue { get; set; }

        public int Skip { get; set; }

        public int Top { get; set; }

        /// <summary>
        /// The total count of the result if it weren't paged.
        /// </summary>
        public int TotalCount { get; set; }
    }
}

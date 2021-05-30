namespace Tellma.Api.Templating
{
    /// <summary>
    /// A <see cref="QueryInfo"/> that encodes an aggregate query.
    /// </summary>
    public class QueryAggregateInfo : QueryInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAggregateInfo"/> class.
        /// </summary>
        /// <param name="collection">The table to query. E.g. "Document".</param>
        /// <param name="definitionId">The definitionId of the query if any.</param>
        /// <param name="select">The OData-like select expression.</param>
        /// <param name="filter">The OData-like filter expression.</param>
        /// <param name="orderby">The OData-like orderby expression.</param>
        /// <param name="top">The OData-like top value.</param>
        /// <param name="skip">The OData-like skip value.</param>
        public QueryAggregateInfo(string collection, int? definitionId, string select, string filter, string having, string orderby, int? top) : base(collection, definitionId)
        {
            Select = select;
            Filter = filter;
            Having = having;
            OrderBy = orderby;
            Top = top;

            // string collection, int? definitionId, string select, string filter, string having, string orderby, int? top
        }

        /// <summary>
        /// The OData-like select expression.
        /// </summary>
        public string Select { get; }

        /// <summary>
        /// The OData-like filter expression.
        /// </summary>
        public string Filter { get; }

        /// <summary>
        /// The OData-like having filter expression.
        /// </summary>
        public string Having { get; }

        /// <summary>
        /// The OData-like orderby expression.
        /// </summary>
        public string OrderBy { get; }

        /// <summary>
        /// The OData-like top value.
        /// </summary>
        public int? Top { get; }

        /// <summary>
        /// The OData-like skip value.
        /// </summary>
        public int? Skip { get; }

        protected override string Encode()
            => $"Aggregate::{Collection}/{DefinitionId}?select={Select}&filter={Filter}&orderby={OrderBy}&top={Top}&skip={Skip}";
    }
}

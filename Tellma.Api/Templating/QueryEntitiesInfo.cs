namespace Tellma.Api.Templating
{
    /// <summary>
    /// A <see cref="QueryInfo"/> that encodes an entities' query.
    /// </summary>
    public class QueryEntitiesInfo : QueryInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryEntitiesInfo"/> class.
        /// </summary>
        /// <param name="collection">The table to query. E.g. "Document".</param>
        /// <param name="definitionId">The definitionId of the query if any.</param>
        /// <param name="filter">The OData-like filter expression.</param>
        /// <param name="orderby">The OData-like orderby expression.</param>
        /// <param name="top">The OData-like top value.</param>
        /// <param name="skip">The OData-like skip value.</param>
        public QueryEntitiesInfo(string collection, int? definitionId, string filter, string orderby, int? top, int? skip) : base(collection, definitionId)
        {
            Filter = filter;
            OrderBy = orderby;
            Top = top;
            Skip = skip;
        }

        /// <summary>
        /// The OData-like filter expression.
        /// </summary>
        public string Filter { get; }

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
            => $"Entities::{Collection}/{DefinitionId}?filter={Filter}&orderby={OrderBy}&top={Top}&skip={Skip}";
    }
}

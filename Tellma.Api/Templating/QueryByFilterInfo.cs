using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// A <see cref="QueryInfo"/> that encodes an entities' query.
    /// </summary>
    public class QueryByFilterInfo : QueryInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryByFilterInfo"/> class.
        /// </summary>
        /// <param name="collection">The table to query. E.g. "Document".</param>
        /// <param name="definitionId">The definitionId of the query if any.</param>
        /// <param name="filter">The OData-like filter expression.</param>
        /// <param name="orderby">The OData-like orderby expression.</param>
        /// <param name="top">The OData-like top value.</param>
        /// <param name="skip">The OData-like skip value.</param>
        /// <param name="ids">The list of Ids of the entities to retrieve. IF this argument is supplied then all the OData-like arguments are ignored.</param>
        public QueryByFilterInfo(string collection, int? definitionId, string filter, string orderby, int? top, int? skip, IList ids) : base(collection, definitionId)
        {
            Filter = filter;
            OrderBy = orderby;
            Top = top;
            Skip = skip;

            // Ids
            ids ??= new List<object>();
            var idsList = new List<object>(ids.Count);
            foreach (var id in ids)
            {
                idsList.Add(id);
            }
            Ids = idsList;
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

        /// <summary>
        /// The list of Ids of the entities to retrieve. IF this argument is supplied then all the OData-like arguments are ignored.
        /// </summary>
        public IEnumerable<object> Ids { get; }

        protected override string Encode()
        {
            string idsString = string.Join(",", Ids.OrderBy(id => id));
            return $"{Collection}/{DefinitionId}?filter={Filter}&orderby={OrderBy}&top={Top}&skip={Skip}&{idsString}";
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// A <see cref="QueryInfo"/> that encodes an invocation of <see cref="FactServiceBase{TEntity}.GetEntities(Dto.GetArguments, System.Threading.CancellationToken)"/>
    /// </summary>
    public class QueryByFilterInfo : QueryInfo
    {
        /// <summary>
        /// Constructor of <see cref="QueryByFilterInfo"/>
        /// </summary>
        /// <param name="collection">The table to query. E.g. Document</param>
        /// <param name="definitionId">The definitionId of the query</param>
        /// <param name="filter">The OData-like filter argument</param>
        /// <param name="orderby">The OData-like orderby argument</param>
        /// <param name="top">The OData-like top argument</param>
        /// <param name="skip">The OData-like skip argument</param>
        /// <param name="ids">The list of Ids of the entities to retrieve. IF this argument is supplied then all the OData-like arguments are ignored</param>
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
        /// The OData-like filter argument
        /// </summary>
        public string Filter { get; }

        /// <summary>
        /// The OData-like orderby argument
        /// </summary>
        public string OrderBy { get; }

        /// <summary>
        /// The OData-like top argument
        /// </summary>
        public int? Top { get; }

        /// <summary>
        /// The OData-like skip argument
        /// </summary>
        public int? Skip { get; }

        /// <summary>
        /// The list of Ids of the entities to retrieve. IF this argument is supplied then all the OData-like arguments are ignored
        /// </summary>
        public IEnumerable<object> Ids { get; }

        protected override string Encode()
        {
            string idsString = string.Join(",", Ids.OrderBy(id => id));
            return $"{Collection}/{DefinitionId}?filter={Filter}&orderby={OrderBy}&top={Top}&skip={Skip}&{idsString}";
        }
    }
}

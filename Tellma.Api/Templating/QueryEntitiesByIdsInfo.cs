using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// A <see cref="QueryInfo"/> that encodes a query of entities' by their Ids.
    /// </summary>
    public class QueryEntitiesByIdsInfo : QueryInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryEntitiesByIdsInfo"/> class.
        /// </summary>
        /// <param name="collection">The table to query. E.g. "Document".</param>
        /// <param name="definitionId">The definitionId of the query if any.</param>
        /// <param name="ids">The list of Ids of the entities to retrieve.</param>
        public QueryEntitiesByIdsInfo(string collection, int? definitionId, IList ids) : base(collection, definitionId)
        {

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
        /// The list of Ids of the entities to retrieve.
        /// </summary>
        public IList Ids { get; }

        protected override string Encode()
            => $"Entities::{Collection}/{DefinitionId}?{string.Join(",", GetIds().OrderBy(id => id))}";

        /// <summary>
        /// To keep the C# compiler happy.
        /// </summary>
        private IEnumerable<object> GetIds() => Ids.Cast<object>();
    }
}

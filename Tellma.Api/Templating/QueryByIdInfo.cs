using System;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// A <see cref="QueryInfo"/> that encodes a single entity query.
    /// </summary>
    public class QueryByIdInfo : QueryInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryByIdInfo"/>.
        /// </summary>
        /// <param name="collection">The table to query. E.g. "Document".</param>
        /// <param name="definitionId">The definitionId of the query if any.</param>
        /// <param name="id">The id of the entity to retrieve.</param>
        public QueryByIdInfo(string collection, int? definitionId, string id) : base(collection, definitionId)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>
        /// The id of the entity to retrieve.
        /// </summary>
        public string Id { get; }

        protected override string Encode()
        {
            return $"{Collection}/{DefinitionId}/{Id}";
        }
    }
}

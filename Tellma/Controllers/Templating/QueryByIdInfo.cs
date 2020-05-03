using System;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// A <see cref="QueryInfo"/> that encodes an invocation of <see cref="FactGetByIdControllerBase{TEntity, TKey}.GetById(TKey, Dto.GetByIdArguments, System.Threading.CancellationToken)"/>
    /// </summary>
    public class QueryByIdInfo : QueryInfo
    {
        public QueryByIdInfo(string collection, string definitionId, string id) : base(collection, definitionId)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>
        /// The id of the entity to retrieve
        /// </summary>
        public string Id { get; }

        protected override string Encode()
        {
            return $"{Collection}/{DefinitionId}/{Id}";
        }
    }
}

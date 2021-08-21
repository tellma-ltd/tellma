using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using Queryex-style arguments
    /// and allow selecting a certain record by Id.
    /// </summary>
    public abstract class FactTreeServiceBase<TEntity, TKey> : FactGetByIdServiceBase<TEntity, TKey>
        where TEntity : EntityWithKey<TKey>
    {
        #region Lifecycle

        /// <summary>
        /// Initializes a new instance of the <see cref="FactTreeServiceBase{TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="deps">The service dependencies.</param>
        public FactTreeServiceBase(FactServiceDependencies deps) : base(deps)
        {
        }

        #endregion

        #region API

        /// <summary>
        /// Returns a list of entities as per the specifications in the <see cref="GetChildrenArguments{TKey}"/>.
        /// </summary>
        public virtual async Task<EntitiesResult<TEntity>> GetChildrenOf(GetChildrenArguments<TKey> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Parse the parameters
            var expand = ExpressionExpand.Parse(args.Expand);
            var select = ParseSelect(args.Select);
            var filter = ExpressionFilter.Parse(args.Filter);
            var orderby = ExpressionOrderBy.Parse("Node");
            var ids = args.I ?? new List<TKey>();

            // Load the data
            var data = await GetEntitiesByCustomQuery(q => q.FilterByParentIds(ids, args.Roots).Filter(filter), expand, select, orderby, null, cancellation);

            // Transform and Return
            return new EntitiesResult<TEntity>(data, data.Count);
        }

        #endregion
    }
}

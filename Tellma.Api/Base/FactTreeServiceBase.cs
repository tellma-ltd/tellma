using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Common;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id
    /// </summary>
    public abstract class FactTreeServiceBase<TEntity, TKey> : FactGetByIdServiceBase<TEntity, TKey>
        where TEntity : EntityWithKey<TKey>
    {
        public FactTreeServiceBase(IServiceProvider sp) : base(sp)
        {
        }

        /// <summary>
        /// Returns a list of entities as per the specifications in the <see cref="GetChildrenArguments{TKey}"/>
        /// </summary>
        public virtual async Task<(List<TEntity>, Extras)> GetChildrenOf(GetChildrenArguments<TKey> args, CancellationToken cancellation)
        {
            // Parse the parameters
            var expand = ExpressionExpand.Parse(args.Expand);
            var select = ParseSelect(args.Select);
            var filter = ExpressionFilter.Parse(args.Filter);
            var orderby = ExpressionOrderBy.Parse("Node");
            var ids = args.I ?? new List<TKey>();

            // Load the data
            var data = await GetEntitiesByCustomQuery(q => q.FilterByParentIds(ids, args.Roots).Filter(filter), expand, select, orderby, null, cancellation);
            var extras = await GetExtras(data, cancellation);

            // Transform and Return
            return (data, extras);
        }
    }
}

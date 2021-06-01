using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id, as well as updating, deleting, deleting with descendants
    /// and importing lists of that entity.
    /// </summary>
    public abstract class CrudTreeServiceBase<TEntityForSave, TEntity, TKey> : CrudServiceBase<TEntityForSave, TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>, new()
        where TEntity : EntityWithKey<TKey>, new()
    {
        #region Lifecycle

        /// <summary>
        /// Initializes a new instance of the <see cref="CrudTreeServiceBase{TEntityForSave, TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="deps">The service dependencies.</param>
        public CrudTreeServiceBase(ServiceDependencies deps) : base(deps)
        {
        }

        #endregion

        #region API

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

        /// <summary>
        /// Deletes the current node and all the nodes descending from it
        /// </summary>
        public virtual async Task DeleteWithDescendants(List<TKey> ids)
        {
            if (ids == null || !ids.Any())
            {
                return;
            }

            var deleteFilter = await UserPermissionsFilter(PermissionActions.Delete, cancellation: default);
            ids = await CheckActionPermissionsBefore(deleteFilter, ids);
            await ValidateDeleteWithDescendantsAsync(ids);
            if (!ModelState.IsValid)
            {
                throw new ValidationException(ModelState);
            }

            await DeleteWithDescendantsAsync(ids);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Deletes the entities specified by the list of Ids
        /// </summary>
        protected abstract Task DeleteWithDescendantsAsync(List<TKey> ids);

        /// <summary>
        /// Validates the delete operation before it happens
        /// </summary>
        protected virtual Task ValidateDeleteWithDescendantsAsync(List<TKey> ids)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}

using Microsoft.Extensions.Localization;
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

        private readonly IStringLocalizer _localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrudTreeServiceBase{TEntityForSave, TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="deps">The service dependencies.</param>
        public CrudTreeServiceBase(CrudServiceDependencies deps) : base(deps)
        {
            _localizer = deps.Localizer;
        }

        #endregion

        #region API

        /// <summary>
        /// Returns a list of entities as per the specifications in the <see cref="GetChildrenArguments{TKey}"/>.
        /// </summary>
        public virtual async Task<(List<TEntity>, Extras)> GetChildrenOf(GetChildrenArguments<TKey> args, CancellationToken cancellation)
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
            var extras = await GetExtras(data, cancellation);

            // Transform and Return
            return (data, extras);
        }

        /// <summary>
        /// Deletes the current node and all the nodes descending from it after validating that the 
        /// delete is possible. Any validation errors must be added in the model state.
        /// Implementations can assume that the current user has permission to perform the deletion.
        /// </summary>
        public virtual async Task DeleteWithDescendants(List<TKey> ids)
        {
            await Initialize();

            if (ids == null || !ids.Any())
            {
                return;
            }

            var deleteFilter = await UserPermissionsFilter(PermissionActions.Delete, cancellation: default);
            ids = await CheckActionPermissionsBefore(deleteFilter, ids);

            // Transaction
            using var trx = TransactionFactory.ReadCommitted();

            try
            {
                await DeleteWithDescendantsAsync(ids);

                trx.Complete();
            }
            catch (ForeignKeyViolationException)
            {
                // Suppress the existing transaction since it was aborted
                using var suppress = TransactionFactory.Suppress();
                var meta = await GetMetadata(cancellation: default);
                suppress.Complete();

                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Implementations perform three steps:<br/>
        /// 1) Validate that all entities whose Id is one of the given <paramref name="ids"/> can indeed be deleted together with all its descendants. <br/>
        /// 2) If invalid: throws a <see cref="ValidationException"/> containing all the errors. <br/>
        /// 3) If valid: delete from the database all entities whose Id is one of the given <paramref name="ids"/> as well as all their descendants.<br/>
        /// 4) Any non transactional side effects at the end (optional).
        /// <para/>
        /// Note: the call to this method is already wrapped inside a transaction, the user is already trusted
        /// to have the necessary permissions to delete. Also the call is wrapped inside a try that catches any
        /// <see cref="ForeignKeyViolationException"/> and translates it into an appropriate error message.
        /// </summary>
        protected abstract Task DeleteWithDescendantsAsync(List<TKey> ids);

        #endregion
    }
}

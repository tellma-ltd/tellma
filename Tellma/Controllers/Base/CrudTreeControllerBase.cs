using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id, as well as updating, deleting, deleting with descendants
    /// and importing lists of that entity
    /// </summary>
    public abstract class CrudTreeControllerBase<TEntityForSave, TEntity, TKey> : CrudControllerBase<TEntityForSave, TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>, new()
        where TEntity : EntityWithKey<TKey>, new()
    {
        public CrudTreeControllerBase(IServiceProvider sp) : base(sp)
        {
        }

        // IMPORTANT: Children-of is replicated in FactTreeControllerBase, please keep them in sync
        [HttpGet("children-of")]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> GetChildrenOf([FromQuery] GetChildrenArguments<TKey> args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Calculate server time at the very beginning for consistency
                var serverTime = DateTimeOffset.UtcNow;

                // Load the data
                var service = GetCrudTreeService();
                var (data, extras) = await service.GetChildrenOf(args, cancellation);

                var result = TransformToEntitiesResponse(data, extras, serverTime, cancellation);
                return Ok(result);
            }, _logger);
        }

        [HttpDelete("with-descendants")]
        public virtual async Task<ActionResult> DeleteWithDescendants([FromQuery] List<TKey> i)
        {
            // "i" parameter is given a short name to allow a large number of
            // ids to be passed in the query string before the url size limit
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Calculate server time at the very beginning for consistency
                var serverTime = DateTimeOffset.UtcNow;

                // Load the data
                var service = GetCrudTreeService();
                await service.DeleteWithDescendants(i);
                return Ok();
            }, _logger);
        }

        protected override CrudServiceBase<TEntityForSave, TEntity, TKey> GetCrudService()
        {
            return GetCrudTreeService();
        }

        protected abstract CrudTreeServiceBase<TEntityForSave, TEntity, TKey> GetCrudTreeService();
    }

    public abstract class CrudTreeServiceBase<TEntityForSave, TEntity, TKey> : CrudServiceBase<TEntityForSave, TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>, new()
        where TEntity : EntityWithKey<TKey>, new()
    {
        public CrudTreeServiceBase(IServiceProvider sp) : base(sp)
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

        /// <summary>
        /// Deletes the current node and all the nodes descending from it
        /// </summary>
        public virtual async Task DeleteWithDescendants(List<TKey> ids)
        {
            if (ids == null || !ids.Any())
            {
                return;
            }

            var deleteFilter = await UserPermissionsFilter(Constants.Delete, cancellation: default);
            ids = await CheckActionPermissionsBefore(deleteFilter, ids);
            await ValidateDeleteWithDescendantsAsync(ids);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            await DeleteWithDescendantsAsync(ids);
        }

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
    }
}

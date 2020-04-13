using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;

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
        private readonly ILogger _logger;

        public CrudTreeControllerBase(ILogger logger, IStringLocalizer localizer) : base(logger, localizer)
        {
            _logger = logger;
        }

        // Children-of is replicated in FactTreeControllerBase, please keep them in sync
        [HttpGet("children-of")]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> GetChildrenOf([FromQuery] GetChildrenArguments<TKey> args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await GetChildrenOf_Impl(args, cancellation);
                return Ok(result);
            }, _logger);
        }

        /// <summary>
        /// Returns a single entity as per the ID and specifications in the get request
        /// </summary>
        protected virtual async Task<EntitiesResponse<TEntity>> GetChildrenOf_Impl(GetChildrenArguments<TKey> args, CancellationToken cancellation)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Parse the parameters
            var expand = ExpandExpression.Parse(args.Expand);
            var select = SelectExpression.Parse(args.Select);
            var filter = FilterExpression.Parse(args.Filter);
            var orderby = OrderByExpression.Parse("Node");
            var ids = args.I ?? new List<TKey>();

            // Load the data
            var data = await LoadDataByCustomQuery(q => q.FilterByParentIds(ids, args.Roots).Filter(filter), expand, select, orderby, cancellation);
            var extras = await GetExtras(data, cancellation);

            // Transform and Return
            return TransformToEntitiesResponse(data, extras, serverTime);
        }

        [HttpDelete("with-descendants")]
        public virtual async Task<ActionResult> DeleteWithDescendants([FromQuery] List<TKey> i)
        {
            // "i" parameter is given a short name to allow a large number of
            // ids to be passed in the query string before the url size limit
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                await DeleteWithDescendants_Impl(i);
                return Ok();
            }, _logger);
        }

        /// <summary>
        /// Deletes the current node and all the nodes descending from it
        /// </summary>
        protected virtual async Task DeleteWithDescendants_Impl(List<TKey> ids)
        {
            if (ids == null || !ids.Any())
            {
                return;
            }

            await CheckActionPermissions(Constants.Delete, ids.ToArray());
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

using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id
    /// </summary>
    public abstract class FactGetByIdControllerBase<TEntity, TKey> : FactWithIdControllerBase<TEntity, TKey>
        where TEntity : EntityWithKey<TKey>
    {
        // Private Fields
        private readonly ILogger _logger;

        // Constructor
        public FactGetByIdControllerBase(ILogger logger, IStringLocalizer localizer) : base(logger, localizer)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<GetByIdResponse<TEntity>>> GetById(TKey id, [FromQuery] GetByIdArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await GetByIdImplAsync(id, args);
                return Ok(result);
            }, _logger);
        }

        /// <summary>
        /// Returns a single entity as per the ID and specifications in the get request
        /// </summary>
        protected virtual async Task<GetByIdResponse<TEntity>> GetByIdImplAsync(TKey id, [FromQuery] GetByIdArguments args)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Parse the parameters
            var expand = ExpandExpression.Parse(args?.Expand);
            var select = SelectExpression.Parse(args?.Select);

            // Prepare the odata query
            var repo = GetRepository();
            var query = repo.Query<TEntity>();

            // Add the filter by Id
            query = query.FilterByIds(id);

            // Check that the entity exists
            int count = await query.CountAsync();
            if (count == 0)
            {
                throw new NotFoundException<TKey>(id);
            }

            // Apply read permissions
            var permissions = await UserPermissions(Constants.Read);
            var permissionsFilter = GetReadPermissionsCriteria(permissions);
            query = query.Filter(permissionsFilter);

            // Apply the expand, which has the general format 'Expand=A,B/C,D'
            var expandedQuery = query.Expand(expand);

            // Apply the select, which has the general format 'Select=A,B/C,D'
            expandedQuery = expandedQuery.Select(select);

            // Load
            var result = await expandedQuery.FirstOrDefaultAsync();
            if (result == null)
            {
                // We already checked for not found earlier,
                // This can only mean lack of permissions
                throw new ForbiddenException();
            }
            
            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            var singleton = new List<TEntity> { result };
            await ApplyReadPermissionsMask(singleton, query, permissions, GetDefaultMask());

            // Get any controller-specific extras
            var extras = await GetExtras(singleton);

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(singleton, expand);

            // Prepare response
            return new GetByIdResponse<TEntity>
            {
                Result = result,
                CollectionName = GetCollectionName(typeof(TEntity)),
                RelatedEntities = relatedEntities,
                Extras = extras,
                ServerTime = serverTime,
            };
        }

        protected async Task<EntitiesResponse<TEntity>> GetByIdListAsync(TKey[] ids, ExpandExpression expand = null, SelectExpression select = null)
        {
            var result = await GetByCustomQuery(q => q.FilterByIds(ids), expand, select);

            // Sort the entities according to the original Ids, as a good practice
            TEntity[] sortedResult = new TEntity[ids.Length];
            Dictionary<TKey, TEntity> resultDic = result.Result.ToDictionary(e => e.Id);
            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                TEntity entity = null;
                if (resultDic.ContainsKey(id))
                {
                    entity = resultDic[id];
                }

                sortedResult[i] = entity;
            }

            result.Result = sortedResult;

            // Return the sorted result
            return result;
        }

        /// <summary>
        /// Returns an entities response based on custom filtering function applied to the query, as well as
        /// optional select and expand arguments, checking the user permissions along the way
        /// </summary>
        /// <param name="filterFunc">Allows you to apply any filteration you like to the query,</param>
        /// <param name="expand">Optional expand argument</param>
        /// <param name="select">Optional select argument</param>
        protected async Task<EntitiesResponse<TEntity>> GetByCustomQuery(Func<Query<TEntity>, Query<TEntity>> filterFunc, ExpandExpression expand, SelectExpression select, OrderByExpression orderby = null)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Prepare a query of the result, and clone it
            var repo = GetRepository();
            var query = repo.Query<TEntity>();

            // Apply custom filter function
            query = filterFunc(query);

            // Expand the result as specified in the OData agruments and load into memory
            var expandedQuery = query.Expand(expand);
            expandedQuery = expandedQuery.Select(select);
            expandedQuery = expandedQuery.OrderBy(orderby ?? OrderByExpression.Parse("Id")); // Required
            var result = await expandedQuery.ToListAsync(); // this is potentially unordered, should that be a concern?

            // Apply the permissions on the result
            var permissions = await UserPermissions(Constants.Read);
            var defaultMask = GetDefaultMask();
            await ApplyReadPermissionsMask(result, query, permissions, defaultMask);

            // Get any controller-specific extras
            var extras = await GetExtras(result);

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(result, expand);

            // Prepare the result in a response object
            return new EntitiesResponse<TEntity>
            {
                Result = result,
                RelatedEntities = relatedEntities,
                CollectionName = GetCollectionName(typeof(TEntity)),
                Extras = extras,
                ServerTime = serverTime,
            };
        }
    }
}

using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
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
            // Parse the parameters
            var expand = ExpandExpression.Parse(args.Expand);
            var select = SelectExpression.Parse(args.Select);

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

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(singleton, expand);

            // Return
            return new GetByIdResponse<TEntity>
            {
                Result = result,
                CollectionName = GetCollectionName(typeof(TEntity)),
                RelatedEntities = relatedEntities
            };
        }

        protected async Task<EntitiesResponse<TEntity>> GetByIdListAsync(TKey[] ids, ExpandExpression expand = null)
        {
            // Prepare a query of the result, and clone it
            var repo = GetRepository();
            var query = repo.Query<TEntity>();

            // Filter by Ids
            query = query.FilterByIds(ids);

            // Expand the result as specified in the OData agruments and load into memory
            var expandedQuery = query.Expand(expand);
            expandedQuery = expandedQuery.OrderBy(OrderByExpression.Parse("Id")); // Required
            var result = await expandedQuery.ToListAsync(); // this is potentially unordered, should that be a concern?

            // Apply the permissions on the result
            var permissions = await UserPermissions(Constants.Read);
            var defaultMask = GetDefaultMask();
            await ApplyReadPermissionsMask(result, query, permissions, defaultMask);

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(result, expand);

            // Sort the entities according to the original Ids, as a good practice
            TEntity[] sortedResult = new TEntity[ids.Length];
            Dictionary<TKey, TEntity> affectedEntitiesDic = result.ToDictionary(e => e.Id);
            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                TEntity entity = null;
                if (affectedEntitiesDic.ContainsKey(id))
                {
                    entity = affectedEntitiesDic[id];
                }

                sortedResult[i] = entity;
            }

            // Prepare the result in a response object
            return new EntitiesResponse<TEntity>
            {
                Result = sortedResult,
                RelatedEntities = relatedEntities,
                CollectionName = GetCollectionName(typeof(TEntity))
            };
        }
    }
}

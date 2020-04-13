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
                var result = await GetByIdImpl(id, args);
                return Ok(result);
            }, _logger);
        }

        /////////////////////////////
        // Endpoint Implementations
        /////////////////////////////

        /// <summary>
        /// Returns the <see cref="GetByIdResponse{TEntity}"/> as per the Id and specifications in <see cref="GetByIdArguments"/>, after verifying the user's permissions
        /// </summary>
        protected virtual async Task<GetByIdResponse<TEntity>> GetByIdImpl(TKey id, GetByIdArguments args)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Load the data
            var entity = await GetByIdLoadData(id, args);

            // Load the extras
            var singleton = new List<TEntity> { entity };
            var extras = await GetExtras(singleton);

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(singleton);

            // Prepare the result in a response object
            return new GetByIdResponse<TEntity>
            {
                Result = entity,
                RelatedEntities = relatedEntities,
                CollectionName = GetCollectionName(typeof(TEntity)),
                Extras = extras,
                ServerTime = serverTime,
            };
        }

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the Id and the specifications in the <see cref="GetByIdArguments"/>, after verifying the user's permissions
        /// </summary>
        protected virtual async Task<TEntity> GetByIdLoadData(TKey id, GetByIdArguments args)
        {
            // Parse the parameters
            var expand = ExpandExpression.Parse(args?.Expand);
            var select = SelectExpression.Parse(args?.Select);

            // Load the data
            var data = await LoadDataByIds(new TKey[] { id }, expand, select);

            // Check that the entity exists, else return NotFound
            var entity = data.SingleOrDefault();
            if (entity == null)
            {
                throw new NotFoundException<TKey>(id);
            }

            // Return
            return entity;
        }

        ///////////////////
        // Helper Methods
        ///////////////////

        /// <summary>
        /// Helper function for all "action" web handlers (like activate and deactivate) that
        /// wish to load a bunch of affected entities via their Ids and return them as an <see cref="EntitiesResponse{TEntity}"/>, after verifying the user's permissions
        /// </summary>
        protected async Task<EntitiesResponse<TEntity>> LoadDataByIdsAndTransform(TKey[] ids, ExpandExpression expand = null, SelectExpression select = null)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Calculate server time at the very beginning for consistency
            var data = await LoadDataByIds(ids, expand, select);

            // Get the extras
            var extras = await GetExtras(data);

            // Transform the entities as an EntitiesResponse
            var response = TransformToEntitiesResponse(data, extras, serverTime);

            // Return
            return response;
        }

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the Ids and the specifications in the <see cref="ExpandExpression"/> and <see cref="SelectExpression"/>, after verifying the user's permissions
        /// </summary>
        protected virtual async Task<List<TEntity>> LoadDataByIds(TKey[] ids, ExpandExpression expand = null, SelectExpression select = null)
        {
            if (ids == null || ids.Length == 0)
            {
                return new List<TEntity>();
            }
            else
            {
                // Load data
                var data = await LoadDataByCustomQuery(q => q.FilterByIds(ids), expand, select);

                // If the data is only 
                if (ids.Length == 1 && data.Count == 1)
                {
                    // No need to sort
                    return data;
                }
                else
                {
                    // Sort the entities according to the original Ids, as a good practice
                    TEntity[] dataSorted = new TEntity[ids.Length];
                    Dictionary<TKey, TEntity> dataDic = data.ToDictionary(e => e.Id);
                    for (int i = 0; i < ids.Length; i++)
                    {
                        var id = ids[i];
                        if (dataDic.TryGetValue(id, out TEntity entity))
                        {
                            dataSorted[i] = entity;
                        }
                    }

                    return dataSorted.ToList();
                }
            }
        }

        /// <summary>
        /// Transforms the data and the other data into an <see cref="EntitiesResponse{TEntity}"/> ready to be served by a web handler, after verifying the user's permissions
        /// </summary>
        protected EntitiesResponse<TEntity> TransformToEntitiesResponse(List<TEntity> data, Dictionary<string, object> extras, DateTimeOffset serverTime)
        {
            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(data);

            // Prepare the result in a response object
            return new EntitiesResponse<TEntity>
            {
                Result = data,
                RelatedEntities = relatedEntities,
                CollectionName = GetCollectionName(typeof(TEntity)),
                Extras = extras,
                ServerTime = serverTime,
            };
        }

        /// <summary>
        /// Returns an <see cref="List{TEntity}"/> based on a custom filtering function applied to the query, as well as
        /// optional select and expand arguments, checking the user permissions along the way
        /// </summary>
        /// <param name="filterFunc">Allows any kind of filtering on the query</param>
        /// <param name="expand">Optional expand argument</param>
        /// <param name="select">Optional select argument</param>
        protected async Task<List<TEntity>> LoadDataByCustomQuery(Func<Query<TEntity>, Query<TEntity>> filterFunc, ExpandExpression expand, SelectExpression select, OrderByExpression orderby = null)
        {
            // Prepare a query of the result, and clone it
            var repo = GetRepository();
            var query = repo.Query<TEntity>();

            // Apply custom filter function
            query = filterFunc(query);

            // Apply read permissions
            var permissions = await UserPermissions(Constants.Read);
            var permissionsFilter = GetReadPermissionsCriteria(permissions);
            query = query.Filter(permissionsFilter);

            // Expand, Select and Order the result as specified in the OData agruments
            var expandedQuery = query.Expand(expand).Select(select).OrderBy(orderby ?? OrderByExpression.Parse("Id")); // Required

            // Load the result into memory
            var data = await expandedQuery.ToListAsync(); // this is potentially unordered, should that be a concern?

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            await ApplyReadPermissionsMask(data, query, permissions, GetDefaultMask());

            // Return
            return data;
        }
    }
}

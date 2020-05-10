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
using System.Threading;

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
        private readonly ILogger _logger;

        public FactGetByIdControllerBase(ILogger logger) : base(logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<GetByIdResponse<TEntity>>> GetById(TKey id, [FromQuery] GetByIdArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Calculate server time at the very beginning for consistency
                var serverTime = DateTimeOffset.UtcNow;

                // Load the data
                var service = GetFactGetByIdService();
                var (entity, extras) = await service.GetById(id, args, cancellation);

                // Load the extras
                var singleton = new List<TEntity> { entity };

                // Flatten and Trim
                var relatedEntities = FlattenAndTrim(singleton, cancellation);

                // Prepare the result in a response object
                var result = new GetByIdResponse<TEntity>
                {
                    Result = entity,
                    RelatedEntities = relatedEntities,
                    CollectionName = GetCollectionName(typeof(TEntity)),
                    Extras = extras,
                    ServerTime = serverTime,
                };
                return Ok(result);
            }, _logger);
        }

        protected override FactWithIdServiceBase<TEntity, TKey> GetFactWithIdService()
        {
            return GetFactGetByIdService();
        }

        protected abstract FactGetByIdServiceBase<TEntity, TKey> GetFactGetByIdService();

        /// <summary>
        /// Transforms the data and the other data into an <see cref="EntitiesResponse{TEntity}"/> ready to be served by a web handler, after verifying the user's permissions
        /// </summary>
        protected EntitiesResponse<TEntity> TransformToEntitiesResponse(List<TEntity> data, Extras extras, DateTimeOffset serverTime, CancellationToken cancellation)
        {
            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(data, cancellation);

            // Prepare the result in a response object
            return new EntitiesResponse<TEntity>
            {
                Result = data,
                RelatedEntities = relatedEntities,
                CollectionName = GetCollectionName(typeof(TEntity)),
                Extras = TransformExtras(extras, cancellation),
                ServerTime = serverTime,
            };
        }

        protected virtual Extras TransformExtras(Extras extras, CancellationToken cancellation)
        {
            return extras;
        }
    }

    public abstract class FactGetByIdServiceBase<TEntity, TKey> : FactWithIdServiceBase<TEntity, TKey>
        where TEntity : EntityWithKey<TKey>
    {
        // Private Fields
        public FactGetByIdServiceBase(IStringLocalizer localizer) : base(localizer)
        {
        }

        /// <summary>
        /// Returns a <see cref="TEntity"/> as per the Id and the specifications in the <see cref="GetByIdArguments"/>, after verifying the user's permissions
        /// </summary>
        public virtual async Task<(TEntity, Extras)> GetById(TKey id, GetByIdArguments args, CancellationToken cancellation)
        {
            // Parse the parameters
            var expand = ExpandExpression.Parse(args?.Expand);
            var select = ParseSelect(args?.Select);

            // Load the data
            var data = await GetEntitiesByIds(new List<TKey> { id }, expand, select, cancellation);
            var extras = await GetExtras(data, cancellation);

            // Check that the entity exists, else return NotFound
            var entity = data.SingleOrDefault();
            if (entity == null)
            {
                throw new NotFoundException<TKey>(id);
            }

            // Return
            return (entity, extras);
        }

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the Ids and the specifications in the <see cref="SelectExpandArguments"/>, after verifying the user's permissions
        /// </summary>
        public virtual async Task<(List<TEntity>, Extras)> GetByIds(List<TKey> ids, SelectExpandArguments args, CancellationToken cancellation)
        {
            // Parse the parameters
            var expand = ExpandExpression.Parse(args?.Expand);
            var select = ParseSelect(args?.Select);

            // Load the data
            var data = await GetEntitiesByIds(ids, expand, select, cancellation);
            var extras = await GetExtras(data, cancellation);

            return (data, extras);
        }

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> as per the Ids and the specifications in the <see cref="ExpandExpression"/> and <see cref="SelectExpression"/>, after verifying the user's permissions
        /// </summary>
        protected virtual async Task<List<TEntity>> GetEntitiesByIds(List<TKey> ids, ExpandExpression expand, SelectExpression select, CancellationToken cancellation)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<TEntity>();
            }
            else
            {
                // Load data
                var data = await GetEntitiesByCustomQuery(q => q.FilterByIds(ids), expand, select, null, cancellation);

                // If the data is only 
                if (ids.Count == 1 && data.Count == 1)
                {
                    // No need to sort
                    return data;
                }
                else
                {
                    // Sort the entities according to the original Ids, as a good practice
                    TEntity[] dataSorted = new TEntity[ids.Count];
                    Dictionary<TKey, TEntity> dataDic = data.ToDictionary(e => e.Id);
                    for (int i = 0; i < ids.Count; i++)
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
        /// Returns an <see cref="List{TEntity}"/> based on a custom filtering function applied to the query, as well as
        /// optional select and expand arguments, checking the user permissions along the way
        /// </summary>
        /// <param name="filterFunc">Allows any kind of filtering on the query</param>
        /// <param name="expand">Optional expand argument</param>
        /// <param name="select">Optional select argument</param>
        protected async Task<List<TEntity>> GetEntitiesByCustomQuery(Func<Query<TEntity>, Query<TEntity>> filterFunc, ExpandExpression expand, SelectExpression select, OrderByExpression orderby, CancellationToken cancellation)
        {
            // Prepare a query of the result, and clone it
            var repo = GetRepository();
            var query = repo.Query<TEntity>();

            // Apply custom filter function
            query = filterFunc(query);

            // Apply read permissions
            var permissions = await UserPermissions(Constants.Read, cancellation);
            var permissionsFilter = GetReadPermissionsCriteria(permissions);
            query = query.Filter(permissionsFilter);

            // Expand, Select and Order the result as specified in the OData agruments
            var expandedQuery = query.Expand(expand).Select(select).OrderBy(orderby ?? OrderByExpression.Parse("Id")); // Required

            // Load the result into memory
            var data = await expandedQuery.ToListAsync(cancellation); // this is potentially unordered, should that be a concern?

            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            await ApplyReadPermissionsMask(data, query, permissions, GetDefaultMask(), cancellation);

            // Return
            return data;
        }
    }
}

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
                    Extras = TransformExtras(extras, cancellation),
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
    }

    public abstract class FactGetByIdServiceBase<TEntity, TKey> : FactWithIdServiceBase<TEntity, TKey>, IFactGetByIdServiceBase
        where TEntity : EntityWithKey<TKey>
    {
        // Private Fields
        public FactGetByIdServiceBase(IServiceProvider sp) : base(sp)
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

        async Task<(EntityWithKey, Extras)> IFactGetByIdServiceBase.GetById(object id, GetByIdArguments args, CancellationToken cancellation)
        {
            return await GetById((TKey)id, args, cancellation);
        }
    }

    public interface IFactGetByIdServiceBase : IFactWithIdService
    {
        Task<(EntityWithKey, Extras)> GetById(object id, GetByIdArguments args, CancellationToken cancellation);
    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Common;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters.
    /// </summary>
    public abstract class FactWithIdControllerBase<TEntity, TKey> : FactControllerBase<TEntity>
        where TEntity : EntityWithKey<TKey>
    {
        // Constructor
        public FactWithIdControllerBase(IServiceProvider sp) : base(sp)
        {
        }

        [HttpGet("by-ids")]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> GetByIds([FromQuery] GetByIdsArguments<TKey> args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Calculate server time at the very beginning for consistency
                var serverTime = DateTimeOffset.UtcNow;

                // Load the data
                var service = GetFactWithIdService();
                var (entities, extras) = await service.GetByIds(args.I, args, Constants.Read, cancellation);

                // Flatten and Trim
                var relatedEntities = FlattenAndTrim(entities, cancellation);

                // Prepare the result in a response object
                var result = new EntitiesResponse<TEntity>
                {
                    Result = entities,
                    RelatedEntities = relatedEntities,
                    CollectionName = ControllerUtilities.GetCollectionName(typeof(TEntity)),
                    Extras = extras,
                    ServerTime = serverTime,
                };
                return Ok(result);
            }, 
            _logger);
        }

        protected override FactServiceBase<TEntity> GetFactService()
        {
            return GetFactWithIdService();
        }

        protected abstract FactWithIdServiceBase<TEntity, TKey> GetFactWithIdService();

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
                CollectionName = ControllerUtilities.GetCollectionName(typeof(TEntity)),
                Extras = TransformExtras(extras, cancellation),
                ServerTime = serverTime,
            };
        }
    }
}

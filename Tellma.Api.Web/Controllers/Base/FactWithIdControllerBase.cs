using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Common;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using Queryex-style arguments.
    /// </summary>
    public abstract class FactWithIdControllerBase<TEntity, TKey, TEntitiesResult> : FactControllerBase<TEntity, TEntitiesResult>
        where TEntitiesResult : EntitiesResult<TEntity>
        where TEntity : EntityWithKey<TKey>
    {
        [HttpGet("by-ids")]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> GetByIds([FromQuery] GetByIdsArguments<TKey> args, CancellationToken cancellation)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Load the data
            var service = GetFactWithIdService();
            var result = await service.GetByIds(args.I, args, cancellation);

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(result.Data, cancellation);
            var extras = CreateExtras(result);

            // Prepare the result in a response object
            var response = new EntitiesResponse<TEntity>
            {
                Result = result.Data,
                RelatedEntities = relatedEntities,
                CollectionName = ControllerUtilities.GetCollectionName(typeof(TEntity)),
                Extras = extras,
                ServerTime = serverTime,
            };

            return Ok(response);
        }

        protected override FactServiceBase<TEntity, TEntitiesResult> GetFactService()
        {
            return GetFactWithIdService();
        }

        protected abstract FactWithIdServiceBase<TEntity, TKey, TEntitiesResult> GetFactWithIdService();

        /// <summary>
        /// Transforms the data and the other data into an <see cref="EntitiesResponse{TEntity}"/> ready to be served by a web handler, after verifying the user's permissions
        /// </summary>
        protected EntitiesResponse<TEntity> TransformToEntitiesResponse(TEntitiesResult result, DateTimeOffset serverTime, CancellationToken cancellation)
        {
            var data = result.Data;

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(data, cancellation);

            // Prepare the result in a response object
            return new EntitiesResponse<TEntity>
            {
                Result = data,
                RelatedEntities = relatedEntities,
                CollectionName = ControllerUtilities.GetCollectionName(typeof(TEntity)),
                Extras = CreateExtras(result),
                ServerTime = serverTime,
            };
        }
    }

    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using Queryex-style arguments.
    /// </summary>
    public abstract class FactWithIdControllerBase<TEntity, TKey> : FactWithIdControllerBase<TEntity, TKey, EntitiesResult<TEntity>>
        where TEntity : EntityWithKey<TKey>
    {
    }
}

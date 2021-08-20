using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using Queryex-style arguments
    /// and allow selecting a certain record by Id.
    /// </summary>
    public abstract class FactGetByIdControllerBase<TEntity, TKey, TEntitiesResult, TEntityResult> : FactWithIdControllerBase<TEntity, TKey, TEntitiesResult>
        where TEntitiesResult : EntitiesResult<TEntity>
        where TEntityResult : EntityResult<TEntity>
        where TEntity : EntityWithKey<TKey>
    {
        [HttpGet("{id}")]
        public virtual async Task<ActionResult<GetByIdResponse<TEntity>>> GetById(TKey id, [FromQuery] GetByIdArguments args, CancellationToken cancellation)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Load the data + extras
            var service = GetFactGetByIdService();
            var result = await service.GetById(id, args, cancellation);
            var entity = result.Entity;

            // Flatten and Trim
            var singleton = new List<TEntity> { entity };
            var relatedEntities = FlattenAndTrim(singleton, cancellation);

            // Prepare the result in a response object
            var response = new GetByIdResponse<TEntity>
            {
                Result = entity,
                RelatedEntities = relatedEntities,
                CollectionName = ControllerUtilities.GetCollectionName(typeof(TEntity)),
                Extras = CreateExtras(result),
                ServerTime = serverTime,
            };

            return Ok(response);
        }


        [HttpGet("{id}/print/{templateId}")]
        public async Task<ActionResult> PrintById(TKey id, int templateId, [FromQuery] PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            var service = GetFactGetByIdService();
            var (fileBytes, fileName) = await service.PrintById(id, templateId, args, cancellation);
            var contentType = ControllerUtilities.ContentType(fileName);

            return File(fileContents: fileBytes, contentType: contentType, fileName);
        }

        protected override FactWithIdServiceBase<TEntity, TKey, TEntitiesResult> GetFactWithIdService()
        {
            return GetFactGetByIdService();
        }

        protected abstract FactGetByIdServiceBase<TEntity, TKey, TEntitiesResult, TEntityResult> GetFactGetByIdService();

        protected virtual Extras CreateExtras(TEntityResult result) => null;
    }

    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using Queryex-style arguments.
    /// </summary>
    public abstract class FactGetByIdControllerBase<TEntity, TKey> : FactGetByIdControllerBase<TEntity, TKey, EntitiesResult<TEntity>, EntityResult<TEntity>>
        where TEntity : EntityWithKey<TKey>
    {
    }
}

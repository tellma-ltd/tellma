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
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id.
    /// </summary>
    public abstract class FactGetByIdControllerBase<TEntity, TKey> : FactWithIdControllerBase<TEntity, TKey>
        where TEntity : EntityWithKey<TKey>
    {
        public FactGetByIdControllerBase(IServiceProvider sp) : base(sp)
        {
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<GetByIdResponse<TEntity>>> GetById(TKey id, [FromQuery] GetByIdArguments args, CancellationToken cancellation)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Load the data + extras
            var service = GetFactGetByIdService();
            var (entity, extras) = await service.GetById(id, args, cancellation);

            // Flatten and Trim
            var singleton = new List<TEntity> { entity };
            var relatedEntities = FlattenAndTrim(singleton, cancellation);

            // Prepare the result in a response object
            var result = new GetByIdResponse<TEntity>
            {
                Result = entity,
                RelatedEntities = relatedEntities,
                CollectionName = ControllerUtilities.GetCollectionName(typeof(TEntity)),
                Extras = TransformExtras(extras, cancellation),
                ServerTime = serverTime,
            };

            return Ok(result);
        }


        [HttpGet("{id}/print/{templateId}")]
        public async Task<ActionResult> PrintById(TKey id, int templateId, [FromQuery] PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            var service = GetFactGetByIdService();
            var (fileBytes, fileName) = await service.PrintById(id, templateId, args, cancellation);
            var contentType = ControllerUtilities.ContentType(fileName);

            return File(fileContents: fileBytes, contentType: contentType, fileName);
        }

        protected override FactWithIdServiceBase<TEntity, TKey> GetFactWithIdService()
        {
            return GetFactGetByIdService();
        }

        protected abstract FactGetByIdServiceBase<TEntity, TKey> GetFactGetByIdService();
    }
}

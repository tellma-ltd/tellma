using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
            var relatedEntities = Flatten(result.Data, cancellation);
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

        [HttpGet("print-entities/{templateId:int}")]
        public async Task<FileContentResult> PrintEntities(int templateId, [FromQuery] PrintEntitiesArguments<TKey> args, CancellationToken cancellation)
        {
            var service = GetFactWithIdService();
            var result = await service.PrintEntities(templateId, args, cancellation);

            var fileBytes = result.FileBytes;
            var fileName = result.FileName;
            var contentType = ControllerUtilities.ContentType(fileName);
            Response.Headers.Add("x-filename", fileName);

            return File(fileContents: fileBytes, contentType: contentType, fileName);
        }

        [HttpGet("email-entities-preview/{templateId:int}")]
        public async Task<ActionResult<EmailCommandPreview>> EmailCommandPreviewEntities(int templateId, [FromQuery] PrintEntitiesArguments<TKey> args, CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var service = GetFactWithIdService();
            var result = await service.EmailCommandPreviewEntities(templateId, args, cancellation);

            return Ok(result);
        }

        [HttpGet("email-entities-preview/{templateId:int}/{index:int}")]
        public async Task<ActionResult<EmailPreview>> EmailPreviewEntities(int templateId, int index, [FromQuery] PrintEntitiesArguments<TKey> args, CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var service = GetFactWithIdService();
            var result = await service.EmailPreviewEntities(templateId, index, args, cancellation);

            return Ok(result);
        }

        [HttpPut("email-entities/{templateId:int}")]
        public async Task<ActionResult> EmailEntities(int templateId, [FromQuery] PrintEntitiesArguments<TKey> args, [FromBody] EmailCommandVersions versions, CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var service = GetFactWithIdService();
            await service.SendByEmail(templateId, args, versions, cancellation);

            return Ok();
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
            var relatedEntities = Flatten(data, cancellation);

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

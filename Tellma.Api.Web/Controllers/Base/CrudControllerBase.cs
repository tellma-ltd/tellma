using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// and allow selecting a certain record by Id, as well as updating, deleting and importing lists
    /// of that entity.
    /// </summary>
    public abstract class CrudControllerBase<TEntityForSave, TEntity, TKey, TEntitiesResult, TEntityResult> : FactGetByIdControllerBase<TEntity, TKey, TEntitiesResult, TEntityResult>
        where TEntitiesResult : EntitiesResult<TEntity>
        where TEntityResult : EntityResult<TEntity>
        where TEntityForSave : EntityWithKey<TKey>
        where TEntity : EntityWithKey<TKey>
    {
        // HTTP Methods

        [HttpPost]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> Save([FromBody] List<TEntityForSave> entities, [FromQuery] SaveArguments args)
        {
            // Note here we use lists https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=netcore-2.1
            // since the order is semantically relevant for reporting validation errors

            // Basic sanity check, to prevent null entities
            if (entities == null && !ModelState.IsValid)
            {
                if (ModelState.IsValid)
                {
                    return BadRequest("Request Body is empty.");
                }
                else
                {
                    return UnprocessableEntity(ModelState);
                }
            }

            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Load the data
            var service = GetCrudService();
            var result = await service.Save(entities, args);

            await OnSuccessfulSave(result);

            // Transform it and return the result
            if (args?.ReturnEntities ?? false)
            {
                // Transform the entities as an EntitiesResponse
                var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);

                // Return the response
                return Ok(response);
            }
            else
            {
                // Return 200
                return Ok();
            }
        }

        [HttpDelete]
        public virtual async Task<ActionResult> Delete([FromQuery] List<TKey> i)
        {
            // "i" parameter is given a short name to allow a large number of
            // ids to be passed in the query string before the url size limit
            var service = GetCrudService();
            await service.Delete(ids: i);

            await OnSuccessfulDelete(ids: i);

            return Ok();
        }

        [HttpDelete("{id}")]
        public virtual async Task<ActionResult> DeleteId(TKey id)
        {
            var ids = new List<TKey> { id };
            var service = GetCrudService();
            await service.Delete(ids);

            await OnSuccessfulDelete(ids: ids);

            return Ok();
        }

        // Helpers

        protected override FactGetByIdServiceBase<TEntity, TKey, TEntitiesResult, TEntityResult> GetFactGetByIdService()
        {
            return GetCrudService();
        }

        protected abstract CrudServiceBase<TEntityForSave, TEntity, TKey, TEntitiesResult, TEntityResult> GetCrudService();

        /// <summary>
        /// Gives an opportunity for implementations to add headers to the response if a save was successful,
        /// useful to set x-version headers for controllers that cause changes that invalidate the cache
        /// </summary>
        protected virtual Task OnSuccessfulSave(TEntitiesResult result)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gives an opportunity for implementations to add headers to the response if a delete was successful,
        /// useful to set x-version headers for controllers that cause changes that invalidate the cache
        /// </summary>
        protected virtual Task OnSuccessfulDelete(List<TKey> ids)
        {
            return Task.CompletedTask;
        }

        [HttpPost("import"), RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
        public async Task<ActionResult<ImportResponse>> Import([FromQuery] ImportArguments args)
        {
            string contentType = null;
            string fileName = null;
            Stream fileStream = null;

            if (Request.Form.Files.Count > 0)
            {
                IFormFile formFile = Request.Form.Files[0];
                contentType = formFile?.ContentType;
                fileName = formFile?.FileName;
                fileStream = formFile?.OpenReadStream();
            }

            try
            {
                var service = GetCrudService();
                var result = await service.Import(fileStream, fileName, contentType, args);

                var response = new ImportResponse
                {
                    Inserted = result.Inserted,
                    Updated = result.Updated,
                    Milliseconds = result.Milliseconds
                };

                return Ok(response);
            }
            finally
            {
                if (fileStream != null)
                {
                    await fileStream.DisposeAsync();
                }
            }
        }

        [HttpGet("template")]
        public async Task<ActionResult> CsvTemplate(CancellationToken cancellation)
        {
            var service = GetCrudService();
            Stream template = await service.CsvTemplate(cancellation);

            return await Task.FromResult(File(template, MimeTypes.Csv));
        }

        [HttpGet("export")]
        public async Task<ActionResult> Export([FromQuery] ExportArguments args, CancellationToken cancellation)
        {
            var service = GetCrudService();
            Stream fileStream = await service.Export(args, cancellation);

            return File(fileStream, MimeTypes.Csv);
        }

        // TODO: Move to FactControllerBase
        [HttpGet("export-by-ids")]
        public async Task<ActionResult> ExportByIds([FromQuery] ExportByIdsArguments<TKey> args, CancellationToken cancellation)
        {
            var service = GetCrudService();
            Stream fileStream = await service.ExportByIds(args, cancellation);

            return File(fileStream, MimeTypes.Csv);
        }
    }

    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using Queryex-style arguments
    /// and allow selecting a certain record by Id, as well as updating, deleting and importing lists
    /// of that entity.
    /// </summary>
    public abstract class CrudControllerBase<TEntityForSave, TEntity, TKey> : CrudControllerBase<TEntityForSave, TEntity, TKey, EntitiesResult<TEntity>, EntityResult<TEntity>>
        where TEntityForSave : EntityWithKey<TKey>
        where TEntity : EntityWithKey<TKey>
    {
    }
}

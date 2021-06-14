using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Common;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id, as well as updating, deleting and importing lists
    /// of that entity.
    /// </summary>
    public abstract class CrudControllerBase<TEntityForSave, TEntity, TKey> : FactGetByIdControllerBase<TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>, new()
        where TEntity : EntityWithKey<TKey>, new()
    {
        public CrudControllerBase(IServiceProvider sp) : base(sp)
        {
        }

        // HTTP Methods

        [HttpPost]
        public virtual async Task<ActionResult<EntitiesResponse<TEntity>>> Save([FromBody] List<TEntityForSave> entities, [FromQuery] SaveArguments args)
        {
            // Note here we use lists https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=netcore-2.1
            // since the order is semantically relevant for reporting validation errors

            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                using var _ = _instrumentation.Block("Controller Save");

                // Basic sanity check, to prevent null entities
                if (entities == null && !ModelState.IsValid)
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest("Body was empty");
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
                var (data, extras) = await service.Save(entities, args);

                await OnSuccessfulSave(data, extras);


                // Transform it and return the result
                var returnEntities = args?.ReturnEntities ?? false;
                if (returnEntities)
                {
                    using var block = _instrumentation.Block("TransformToEntitiesResponse");

                    // Transform the entities as an EntitiesResponse
                    var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                    // Return the response
                    return Ok(response);
                }
                else
                {
                    // Return 200
                    return Ok();
                }
            }, _logger);
        }

        [HttpDelete]
        public virtual async Task<ActionResult> Delete([FromQuery] List<TKey> i)
        {
            // "i" parameter is given a short name to allow a large number of
            // ids to be passed in the query string before the url size limit
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var service = GetCrudService();
                await service.Delete(ids: i);

                await OnSuccessfulDelete(ids: i);

                return Ok();
            }, _logger);
        }

        [HttpDelete("{id}")]
        public virtual async Task<ActionResult> DeleteId(TKey id)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var ids = new List<TKey> { id };
                var service = GetCrudService();
                await service.Delete(ids);

                await OnSuccessfulDelete(ids: ids);

                return Ok();
            }, _logger);
        }

        // Helpers

        protected override FactGetByIdServiceBase<TEntity, TKey> GetFactGetByIdService()
        {
            return GetCrudService();
        }

        protected abstract CrudServiceBase<TEntityForSave, TEntity, TKey> GetCrudService();

        /// <summary>
        /// Gives an opportunity for implementations to add headers to the response if a save was successful,
        /// useful to set x-version headers for controllers that cause changes that invalidate the cache
        /// </summary>
        protected virtual Task OnSuccessfulSave(List<TEntity> data, Extras extras)
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
        public async Task<ActionResult<ImportResult>> Import([FromQuery] ImportArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                IFormFile formFile = Request.Form.Files.FirstOrDefault();
                var contentType = formFile?.ContentType;
                var fileName = formFile?.FileName;
                using var fileStream = formFile?.OpenReadStream();

                var service = GetCrudService();
                var result = await service.Import(fileStream, fileName, contentType, args);

                return Ok(result);
            }, _logger);
        }

        [HttpGet("template")]
        public async Task<ActionResult> CsvTemplate(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var service = GetCrudService();
                Stream template = await service.CsvTemplate(cancellation);

                return await Task.FromResult(File(template, MimeTypes.Csv));
            }, _logger);
        }

        [HttpGet("export")]
        public async Task<ActionResult> Export([FromQuery] ExportArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var service = GetCrudService();
                Stream fileStream = await service.Export(args, cancellation);

                return File(fileStream, MimeTypes.Csv);
            }, _logger);
        }

        // TODO: Move to FactControllerBase
        [HttpGet("export-by-ids")]
        public async Task<ActionResult> ExportByIds([FromQuery] ExportByIdsArguments<TKey> args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var service = GetCrudService();
                Stream fileStream = await service.ExportByIds(args, cancellation);

                return File(fileStream, MimeTypes.Csv);
            }, _logger);
        }
    }
}

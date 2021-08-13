using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
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
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id, as well as updating, deleting and importing lists
    /// of that entity.
    /// </summary>
    public abstract class CrudControllerBase<TEntityForSave, TEntity, TKey> : FactGetByIdControllerBase<TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>, new()
        where TEntity : EntityWithKey<TKey>, new()
    {
        private readonly IServiceProvider _services;

        public CrudControllerBase(IServiceProvider sp) : base(sp)
        {
            _services = sp;
        }

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
            var (data, extras) = await service.Save(entities, args);

            await OnSuccessfulSave(data, extras);


            // Transform it and return the result
            var returnEntities = args?.ReturnEntities ?? false;
            if (returnEntities)
            {
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
            if (Request.Form.Files.Count == 0)
            {
                var localizer = _services.GetRequiredService<IStringLocalizer<Strings>>();
                return BadRequest(localizer["Error_NoFileWasUploaded"]);
            }

            IFormFile formFile = Request.Form.Files[0];
            var contentType = formFile?.ContentType;
            var fileName = formFile?.FileName;
            using var fileStream = formFile?.OpenReadStream();

            var service = GetCrudService();
            var result = await service.Import(fileStream, fileName, contentType, args);

            return Ok(result);
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
}

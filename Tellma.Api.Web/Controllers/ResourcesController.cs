using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    // Specific API, works with a certain definitionId, and allows read-write
    [Route("api/resources/{definitionId}")]
    [ApplicationController]
    public class ResourcesController : CrudControllerBase<ResourceForSave, Resource, int>
    {
        private readonly ResourcesService _service;

        public ResourcesController(ResourcesService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id, CancellationToken cancellation)
        {
            var (imageId, imageBytes) = await GetService().GetImage(id, cancellation);
            Response.Headers.Add("x-image-id", imageId);

            return File(imageBytes, MimeTypes.Jpeg);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Resource>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().Activate(ids: ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Resource>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().Deactivate(ids: ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        protected override CrudServiceBase<ResourceForSave, Resource, int> GetCrudService()
        {
            _service.SetDefinitionId(DefinitionId);
            return _service;
        }

        private ResourcesService GetService()
        {
            _service.SetDefinitionId(DefinitionId);
            return _service;
        }

        protected int DefinitionId => int.Parse(Request.RouteValues.GetValueOrDefault("definitionId").ToString());
    }

    // Generic API, allows reading all resources

    [Route("api/resources")]
    [ApplicationController]
    public class ResourcesGenericController : FactWithIdControllerBase<Resource, int>
    {
        private readonly ResourcesGenericService _service;

        public ResourcesGenericController(ResourcesGenericService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Resource, int> GetFactWithIdService()
        {
            return _service;
        }
    }
}

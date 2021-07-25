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
    [Route("api/" + BASE_ADDRESS + "{definitionId}")]
    [ApplicationController]
    public class ResourcesController : CrudControllerBase<ResourceForSave, Resource, int>
    {
        public const string BASE_ADDRESS = "resources/";

        private readonly ResourcesService _service;

        public ResourcesController(ResourcesService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (imageId, imageBytes) = await _service.GetImage(id, cancellation);
                Response.Headers.Add("x-image-id", imageId);
                return File(imageBytes, "image/jpeg");

            }, _logger);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Resource>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Activate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            },
            _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Resource>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            },
            _logger);
        }

        protected override CrudServiceBase<ResourceForSave, Resource, int> GetCrudService()
        {
            return _service;
        }
    }

    // Generic API, allows reading all resources

    [Route("api/" + ResourcesController.BASE_ADDRESS)]
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

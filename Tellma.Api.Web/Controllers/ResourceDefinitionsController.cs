using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/resource-definitions")]
    [ApplicationController]
    public class ResourceDefinitionsController : CrudControllerBase<ResourceDefinitionForSave, ResourceDefinition, int>
    {
        private readonly ResourceDefinitionsService _service;

        public ResourceDefinitionsController(ResourceDefinitionsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("update-state")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Close([FromBody] List<int> ids, [FromQuery] UpdateStateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await _service.UpdateState(ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            Response.Headers.Set("x-definitions-version", Constants.Stale);
            if (args.ReturnEntities ?? false)
            {
                return Ok(response);
            }
            else
            {
                return Ok();
            }
        }

        protected override CrudServiceBase<ResourceDefinitionForSave, ResourceDefinition, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(List<ResourceDefinition> data, Extras extras)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulSave(data, extras);
        }

        protected override Task OnSuccessfulDelete(List<int> ids)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulDelete(ids);
        }
    }
}

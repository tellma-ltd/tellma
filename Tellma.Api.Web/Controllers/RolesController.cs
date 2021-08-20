using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/roles")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class RolesController : CrudControllerBase<RoleForSave, Role, int>
    {
        private readonly RolesService _service;

        public RolesController(RolesService service)
        {
            _service = service;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Role>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var result = await _service.Activate(ids: ids, args);
            var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Role>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var result = await _service.Deactivate(ids: ids, args);
            var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);

            return Ok(response);
        }

        protected override CrudServiceBase<RoleForSave, Role, int> GetCrudService()
        {
            return _service;
        }
    }
}

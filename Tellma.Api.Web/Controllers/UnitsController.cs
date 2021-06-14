using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/units")]
    [ApplicationController]
    public class UnitsController : CrudControllerBase<UnitForSave, Unit, int>
    {
        private readonly UnitsService _service;

        public UnitsController(UnitsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Unit>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<Unit>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
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

        protected override CrudServiceBase<UnitForSave, Unit, int> GetCrudService()
        {
            return _service;
        }
    }
}

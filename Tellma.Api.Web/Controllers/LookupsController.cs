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
    [Route("api/lookups/{definitionId:int}")]
    [ApplicationController]
    public class LookupsController : CrudControllerBase<LookupForSave, Lookup, int>
    {
        private readonly LookupsService _service;

        public LookupsController(LookupsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Lookup>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().Activate(ids: ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Lookup>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().Deactivate(ids: ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        protected override CrudServiceBase<LookupForSave, Lookup, int> GetCrudService()
        {
            _service.SetDefinitionId(DefinitionId);
            return _service;
        }

        private LookupsService GetService()
        {
            _service.SetDefinitionId(DefinitionId);
            return _service;
        }

        protected int DefinitionId => int.Parse(Request.RouteValues.GetValueOrDefault("definitionId").ToString());
    }

    [Route("api/lookups")]
    [ApplicationController]
    public class LookupsGenericController : FactWithIdControllerBase<Lookup, int>
    {
        private readonly LookupsGenericService _service;

        public LookupsGenericController(LookupsGenericService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Lookup, int> GetFactWithIdService()
        {
            return _service;
        }
    }
}

﻿using Microsoft.AspNetCore.Mvc;
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
    [Route("api/entry-types")]
    [ApplicationController]
    public class EntryTypesController : CrudTreeControllerBase<EntryTypeForSave, EntryType, int>
    {
        private readonly EntryTypesService _service;

        public EntryTypesController(EntryTypesService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<EntryType>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Activate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);

            }, _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<EntryType>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);

            }, _logger);
        }

        protected override CrudTreeServiceBase<EntryTypeForSave, EntryType, int> GetCrudTreeService()
        {
            return _service;
        }
    }
}
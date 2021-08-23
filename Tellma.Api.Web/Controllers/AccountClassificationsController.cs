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
    [Route("api/account-classifications")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class AccountClassificationsController : CrudTreeControllerBase<AccountClassificationForSave, AccountClassification, int>
    {
        private readonly AccountClassificationsService _service;

        public AccountClassificationsController(AccountClassificationsService service)
        {
            _service = service;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<AccountClassification>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var result = await _service.Activate(ids: ids, args);
            var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<AccountClassification>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var result = await _service.Deactivate(ids: ids, args);
            var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);

            return Ok(response);
        }

        protected override CrudTreeServiceBase<AccountClassificationForSave, AccountClassification, int> GetCrudTreeService()
        {
            return _service;
        }
    }
}

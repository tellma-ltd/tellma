using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/inbox")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class InboxController : FactWithIdControllerBase<InboxRecord, int, InboxResult>
    {
        private readonly InboxService _service;

        public InboxController(InboxService service)
        {
            _service = service;
        }

        [HttpPut("check")]
        public async Task<ActionResult> CheckInbox([FromBody] DateTimeOffset now)
        {
            await _service.CheckInbox(now);
            return Ok();
        }

        protected override FactWithIdServiceBase<InboxRecord, int, InboxResult> GetFactWithIdService()
        {
            return _service;
        }

        protected override Extras CreateExtras(InboxResult result)
        {
            return new Extras
            {
                ["Count"] = result.StatusCount,
                ["UnknownCount"] = result.UnknownCount,
            };
        }
    }
}

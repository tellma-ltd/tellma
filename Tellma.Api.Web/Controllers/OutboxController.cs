using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/outbox")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class OutboxController : FactWithIdControllerBase<OutboxRecord, int>
    {
        private readonly OutboxService _service;

        public OutboxController(OutboxService service)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<OutboxRecord, int> GetFactWithIdService()
        {
            return _service;
        }
    }
}

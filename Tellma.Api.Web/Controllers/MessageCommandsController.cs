using Microsoft.AspNetCore.Mvc;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/message-commands")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class MessageCommandsController : FactGetByIdControllerBase<MessageCommand, int>
    {
        private readonly MessageCommandsService _service;

        public MessageCommandsController(MessageCommandsService service)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<MessageCommand, int> GetFactGetByIdService()
        {
            return _service;
        }
    }
}

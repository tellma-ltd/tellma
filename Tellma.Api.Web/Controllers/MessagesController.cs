using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/messages")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class MessagesController : FactGetByIdControllerBase<MessageForQuery, int>
    {
        private readonly MessagesService _service;

        public MessagesController(MessagesService service)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<MessageForQuery, int> GetFactGetByIdService()
        {
            return _service;
        }
    }
}

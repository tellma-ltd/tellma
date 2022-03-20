using Microsoft.AspNetCore.Mvc;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/email-commands")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class EmailCommandsController : FactGetByIdControllerBase<EmailCommand, int>
    {
        private readonly EmailCommandsService _service;

        public EmailCommandsController(EmailCommandsService service)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<EmailCommand, int> GetFactGetByIdService()
        {
            return _service;
        }
    }
}

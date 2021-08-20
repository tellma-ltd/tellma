using Microsoft.AspNetCore.Mvc;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/sms-messages")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class SmsMessagesController : FactGetByIdControllerBase<SmsMessageForQuery, int>
    {
        private readonly SmsMessagesService _service;

        public SmsMessagesController(SmsMessagesService service)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<SmsMessageForQuery, int> GetFactGetByIdService()
        {
            return _service;
        }
    }
}

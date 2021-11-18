using Microsoft.AspNetCore.Mvc;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/notification-commands")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class NotificationCommandsController : FactGetByIdControllerBase<NotificationCommand, int>
    {
        private readonly NotificationCommandsService _service;

        public NotificationCommandsController(NotificationCommandsService service)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<NotificationCommand, int> GetFactGetByIdService()
        {
            return _service;
        }
    }
}

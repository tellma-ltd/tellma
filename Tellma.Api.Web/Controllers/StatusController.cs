using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Controllers
{
    [Route("api/notifications")]
    [AuthorizeJwtBearer]
    [ApplicationController]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class StatusController : ControllerBase
    {
        private readonly StatusService _service;
        private readonly ILogger<StatusController> _logger;
        private readonly IServiceContextAccessor _accessor;

        public StatusController(StatusService service, ILogger<StatusController> logger, IServiceContextAccessor accessor)
        {
            _service = service;
            _logger = logger;
            _accessor = accessor;
        }

        /// <summary>
        /// When a client connects for the first time, or reconnects after going offline,
        /// it invokes this method to catch up on what it has missed.
        /// </summary>
        /// <returns>A summary of what the client has missed.</returns>
        [HttpGet("recap")]
        public async Task<ActionResult<NotificationSummary>> Recap(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl<NotificationSummary>(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var tenantId = _accessor.TenantId ?? throw new InvalidOperationException("TenantId was not provided");
                var status = await _service.Recap(cancellation);

                var result = new NotificationSummary
                {
                    Inbox = new InboxStatusToSend
                    {
                        Count = status?.Count ?? 0,
                        UnknownCount = status?.UnknownCount ?? 0,
                        UpdateInboxList = true,
                        ServerTime = serverTime,
                        TenantId = tenantId,
                    },
                };

                return Ok(result);
            },
            _logger);
        }
    }
}

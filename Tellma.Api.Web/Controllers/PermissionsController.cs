using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    [AuthorizeJwtBearer]
    [ApplicationController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class PermissionsController : ControllerBase
    {
        private readonly PermissionsService _service;
        private readonly ILogger _logger;

        public PermissionsController(PermissionsService service, ILogger<PermissionsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("client")]
        public virtual async Task<ActionResult<Versioned<PermissionsForClient>>> PermissionsForClient(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.PermissionsForClient(cancellation);
                return Ok(result);
            }, 
            _logger);
        }
    }
}

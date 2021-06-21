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
    [Route("api/admin-permissions")]
    [ApiController]
    [AuthorizeJwtBearer]
    [AdminController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminPermissionsController : ControllerBase
    {
        private readonly AdminPermissionsService _service;
        private readonly ILogger _logger;

        public AdminPermissionsController(AdminPermissionsService service, ILogger<PermissionsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("client")]
        public virtual async Task<ActionResult<Versioned<PermissionsForClient>>> PermissionsForClient(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Retrieve the user permissions and their current version
                var result = await _service.PermissionsForClient(cancellation);
                return Ok(result);
            },
            _logger);
        }
    }
}

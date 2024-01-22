using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Dto;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    [AuthorizeJwtBearer]
    [ApplicationController]
    [ApiVersion("1.0")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class PermissionsController : ControllerBase
    {
        private readonly PermissionsService _service;

        public PermissionsController(PermissionsService service)
        {
            _service = service;
        }

        [HttpGet("client")]
        public virtual async Task<ActionResult<Versioned<PermissionsForClient>>> PermissionsForClient(CancellationToken cancellation)
        {
            var result = await _service.PermissionsForClient(cancellation);
            return Ok(result);
        }
    }
}

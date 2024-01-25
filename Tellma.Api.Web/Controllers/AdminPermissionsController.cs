using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Dto;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Controllers
{
    [Route("api/admin-permissions")]
    [ApiController]
    [AuthorizeJwtBearer]
    [AdminController]
    [ApiVersion("1.0")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminPermissionsController : ControllerBase
    {
        private readonly AdminPermissionsService _service;

        public AdminPermissionsController(AdminPermissionsService service)
        {
            _service = service;
        }

        [HttpGet("client")]
        public virtual async Task<ActionResult<Versioned<AdminPermissionsForClient>>> PermissionsForClient(CancellationToken cancellation)
        {
            // Retrieve the user permissions and their current version
            var result = await _service.PermissionsForClient(cancellation);
            return Ok(result);
        }
    }
}

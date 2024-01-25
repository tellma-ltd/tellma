using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Dto;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Controllers
{
    [Route("api/admin-settings")]
    [AuthorizeJwtBearer]
    [AdminController]
    [ApiController]
    [ApiVersion("1.0")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminSettingsController : ControllerBase
    {
        private readonly AdminSettingsService _service;

        public AdminSettingsController(AdminSettingsService service)
        {
            _service = service;
        }

        // API

        [HttpGet("client")]
        public async Task<ActionResult<Versioned<AdminSettingsForClient>>> SettingsForClient(CancellationToken cancellation)
        {
            // Simply retrieves the cached settings, which were refreshed by AdminApiAttribute
            var result = await _service.SettingsForClient(cancellation);
            return Ok(result);
        }

        [HttpGet("ping")]
        public async Task<ActionResult> Ping(CancellationToken cancellation)
        {
            // If all you want is to check whether the cache versions are fresh, you
            // can use this API which does only that through the service's OnInitialize
            // Simply retrieves the cache versions which are refreshed inside the service's OnInitialize
            await _service.Ping(cancellation);
            return Ok();
        }
    }
}

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
    [Route("api/admin-settings")]
    [AuthorizeJwtBearer]
    [AdminController]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminSettingsController : ControllerBase
    {
        private readonly AdminSettingsService _service;
        private readonly ILogger<GeneralSettingsController> _logger;

        public AdminSettingsController(AdminSettingsService service, ILogger<GeneralSettingsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // API

        [HttpGet("client")]
        public async Task<ActionResult<Versioned<AdminSettingsForClient>>> SettingsForClient(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Simply retrieves the cached settings, which were refreshed by AdminApiAttribute
                var result = await _service.SettingsForClient(cancellation);
                return Ok(result);
            }, 
            _logger);
        }

        [HttpGet("ping")]
        public async Task<ActionResult> Ping(CancellationToken cancellation)
        {
            // If all you want is to check whether the cache versions are fresh, you
            // can use this API which does only that through the service's OnInitialize
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Simply retrieves the cache versions which are refreshed inside the service's OnInitialize
                await _service.Ping(cancellation);
                return Ok();
            },
            _logger);
        }
    }
}

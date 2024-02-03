using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Tellma.Api.Dto;

namespace Tellma.Controllers
{
    [Route("api/global-settings")]
    [ApiController]
    [ApiVersion("1.0")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class GlobalSettingsController : ControllerBase
    {
        private readonly GlobalSettingsProvider _globalSettingsProvider;

        public GlobalSettingsController(GlobalSettingsProvider globalSettingsProvider)
        {
            _globalSettingsProvider = globalSettingsProvider;
        }

        [HttpGet("client")]
        public ActionResult<Versioned<GlobalSettingsForClient>> GlobalSettingsForClient()
        {
            var result = _globalSettingsProvider.GetForClient();
            return Ok(result);
        }

        [HttpGet("ping")]
        public ActionResult Ping()
        {
            // If all you want is to check whether the cached versions of global settings
            // are fresh you can use this API that only does that through the registered filter
            return Ok();
        }
    }
}

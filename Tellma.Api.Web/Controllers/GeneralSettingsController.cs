using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/general-settings")]
    [ApiVersion("1.0")]
    public class GeneralSettingsController : ApplicationSettingsControllerBase<GeneralSettingsForSave, GeneralSettings>
    {
        private readonly GeneralSettingsService _service;

        public GeneralSettingsController(IServiceProvider sp, GeneralSettingsService service) : base(sp)
        {
            _service = service;
        }

        [HttpGet("client")]
        public async Task<ActionResult<Versioned<SettingsForClient>>> SettingsForClient(CancellationToken cancellation)
        {
            var result = await _service.SettingsForClient(cancellation);
            return Ok(result);
        }

        [HttpGet("ping")]
        public async Task<ActionResult> Ping(CancellationToken cancellation)
        {
            // If all you want is to check whether the cached versions of settings and permissions 
            // are fresh you can use this API that only does that through the controller filters
            await _service.Ping(cancellation);
            return Ok();
        }

        protected override ApplicationSettingsServiceBase<GeneralSettingsForSave, GeneralSettings> GetSettingsService()
        {
            return _service;
        }
    }
}

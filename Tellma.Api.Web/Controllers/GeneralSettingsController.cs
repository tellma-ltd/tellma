using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/general-settings")]
    public class GeneralSettingsController : ApplicationSettingsControllerBase<GeneralSettingsForSave, GeneralSettings>
    {
        private readonly GeneralSettingsService _service;
        private readonly ILogger<GeneralSettingsController> _logger;

        public GeneralSettingsController(IServiceProvider sp, GeneralSettingsService service, ILogger<GeneralSettingsController> logger) : base(sp)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("client")]
        public async Task<ActionResult<Versioned<SettingsForClient>>> SettingsForClient(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl<Versioned<SettingsForClient>>(async () =>
            {
                var result = await _service.SettingsForClient(cancellation);
                return Ok(result);
            },
            _logger);
        }

        [HttpGet("ping")]
        public ActionResult Ping()
        {
            // If all you want is to check whether the cached versions of settings and permissions 
            // are fresh you can use this API that only does that through the controller filters
            return Ok();
        }

        protected override ApplicationSettingsServiceBase<GeneralSettingsForSave, GeneralSettings> GetSettingsService()
        {
            return _service;
        }
    }
}
